// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

internal sealed class LanguageExpressionBuilder
{
    private const char Dot = '.';
    private const char OpenBracket = '[';
    private const char CloseBracket = ']';

    private const string DOTWHERE = ".where";
    private const string LESS = "less";
    private const string LESSOREQUAL = "lessOrEqual";
    private const string GREATER = "greater";
    private const string GREATEROREQUAL = "greaterOrEqual";
    private const string COUNT = "count";

    private readonly bool _Debugger;

    private string[] _With;
    private string[] _Type;
    private LanguageExpression _When;
    private string[] _Rule;

    public LanguageExpressionBuilder(bool debugger = true)
    {
        _Debugger = debugger;
    }

    public LanguageExpressionBuilder WithSelector(string[] with)
    {
        if (with == null || with.Length == 0)
            return this;

        _With = with;
        return this;
    }

    public LanguageExpressionBuilder WithType(string[] type)
    {
        if (type == null || type.Length == 0)
            return this;

        _Type = type;
        return this;
    }

    public LanguageExpressionBuilder WithSubselector(LanguageIf subselector)
    {
        if (subselector == null || subselector.Expression == null)
            return this;

        _When = subselector.Expression;
        return this;
    }

    public LanguageExpressionBuilder WithRule(string[] rule)
    {
        if (rule == null || rule.Length == 0)
            return this;

        _Rule = rule;
        return this;
    }

    public LanguageExpressionOuterFn Build(LanguageIf condition)
    {
        return Precondition(Expression(string.Empty, condition.Expression), _With, _Type, Expression(string.Empty, _When), _Rule);
    }

    private static LanguageExpressionOuterFn Precondition(LanguageExpressionOuterFn expression, string[] with, string[] type, LanguageExpressionOuterFn when, string[] rule)
    {
        var fn = expression;
        if (type != null)
            fn = PreconditionType(type, fn);

        if (with != null)
            fn = PreconditionSelector(with, fn);

        if (when != null)
            fn = PreconditionSubselector(when, fn);

        if (rule != null)
            fn = PreconditionRule(rule, fn);

        return fn;
    }

    private static LanguageExpressionOuterFn PreconditionRule(string[] rule, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            // Evaluate selector rule pre-condition
            if (!AcceptsRule(rule))
            {
                context.Debug(PSRuleResources.DebugTargetRuleMismatch);
                return null;
            }
            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn PreconditionSelector(string[] with, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            // Evaluate selector pre-condition
            if (!AcceptsWith(with))
            {
                context.Debug(PSRuleResources.DebugTargetTypeMismatch);
                return null;
            }
            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn PreconditionType(string[] type, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            // Evaluate type pre-condition
            if (!AcceptsType(type))
            {
                context.Debug(PSRuleResources.DebugTargetTypeMismatch);
                return null;
            }
            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn PreconditionSubselector(LanguageExpressionOuterFn subselector, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            try
            {
                context.PushScope(RunspaceScope.Precondition);

                // Evaluate sub-selector pre-condition
                if (!AcceptsSubselector(context, subselector, o))
                {
                    context.Debug(PSRuleResources.DebugTargetSubselectorMismatch);
                    return null;
                }
            }
            finally
            {
                context.PopScope(RunspaceScope.Precondition);
            }
            return fn(context, o);
        };
    }

    private LanguageExpressionOuterFn Expression(string path, LanguageExpression expression)
    {
        if (expression == null)
            return null;

        path = Path(path, expression);
        if (expression is LanguageOperator selectorOperator)
            return Scope(Debugger(Operator(path, selectorOperator), path));
        else if (expression is LanguageCondition selectorCondition)
            return Scope(Debugger(Condition(path, selectorCondition), path));

        throw new InvalidOperationException();
    }

    private static LanguageExpressionOuterFn Scope(LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            RunspaceContext.CurrentThread?.EnterLanguageScope(context.Source);

            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn Condition(string path, LanguageCondition expression)
    {
        var info = new ExpressionInfo(path);
        return (context, o) => expression.Descriptor.Fn(context, info, [expression.Property], o);
    }

    private static string Path(string path, LanguageExpression expression)
    {
        path = string.Concat(path, Dot, expression.Descriptor.Name);
        return path;
    }

    private LanguageExpressionOuterFn Operator(string path, LanguageOperator expression)
    {
        var inner = new List<LanguageExpressionOuterFn>(expression.Children.Count);
        for (var i = 0; i < expression.Children.Count; i++)
        {
            var childPath = string.Concat(path, OpenBracket, i, CloseBracket);
            inner.Add(Expression(childPath, expression.Children[i]));
        }
        var innerA = inner.ToArray();
        var info = new ExpressionInfo(path);

        // Check for sub-selectors
        if (expression.Property == null || expression.Property.Count == 0)
        {
            return (context, o) => expression.Descriptor.Fn(context, info, innerA, o);
        }
        else
        {
            var subselector = expression.Subselector != null ? Expression(string.Concat(path, DOTWHERE), expression.Subselector) : null;
            return (context, o) =>
            {
                ObjectHelper.GetPath(context, o, Value<string>(context, expression.Property["field"]), caseSensitive: false, out object[] items);

                var quantifier = GetQuantifier(expression);
                var pass = 0;

                // If any fail, all fail
                for (var i = 0; items != null && i < items.Length; i++)
                {
                    if (subselector == null || subselector(context, items[i]).GetValueOrDefault(true))
                    {
                        if (!expression.Descriptor.Fn(context, info, innerA, items[i]))
                        {
                            if (quantifier == null)
                                return false;
                        }
                        else
                        {
                            pass++;
                        }
                    }
                }
                return quantifier == null || quantifier(pass);
            };
        }
    }

    /// <summary>
    /// Returns a quantifier function if set for the expression.
    /// </summary>
    private static Func<long, bool> GetQuantifier(LanguageOperator expression)
    {
        if (expression.Property.TryGetLong(GREATEROREQUAL, out var q))
            return (number) => number >= q.Value;

        if (expression.Property.TryGetLong(GREATER, out q))
            return (number) => number > q.Value;

        if (expression.Property.TryGetLong(LESSOREQUAL, out q))
            return (number) => number <= q.Value;

        if (expression.Property.TryGetLong(LESS, out q))
            return (number) => number < q.Value;

        if (expression.Property.TryGetLong(COUNT, out q))
            return (number) => number == q.Value;

        return null;
    }

    [DebuggerStepThrough]
    private static string Value<T>(IExpressionContext context, object v)
    {
        return v as string;
    }

    private LanguageExpressionOuterFn Debugger(LanguageExpressionOuterFn expression, string path)
    {
        return !_Debugger ? expression : ((context, o) => DebuggerFn(context, path, expression, o));
    }

    private static bool? DebuggerFn(IExpressionContext context, string path, LanguageExpressionOuterFn expression, object o)
    {
        var result = expression(context, o);
        var type = context.Kind == ResourceKind.Rule ? 'R' : 'S';
        context.Debug(PSRuleResources.LanguageExpressionTraceP2, type, path, result);
        return result;
    }

    private static bool AcceptsType(string[] type)
    {
        if (type == null)
            return true;

        var comparer = RunspaceContext.CurrentThread.LanguageScope.GetBindingComparer();
        var targetType = RunspaceContext.CurrentThread.RuleRecord.TargetType;
        for (var i = 0; i < type.Length; i++)
        {
            if (comparer.Equals(targetType, type[i]))
                return true;
        }
        return false;
    }

    private static bool AcceptsWith(string[] with)
    {
        if (with == null || with.Length == 0)
            return true;

        for (var i = 0; i < with.Length; i++)
        {
            if (RunspaceContext.CurrentThread.TrySelector(with[i]))
                return true;
        }
        return false;
    }

    private static bool AcceptsSubselector(IExpressionContext context, LanguageExpressionOuterFn subselector, object o)
    {
        return subselector == null || subselector.Invoke(context, o).GetValueOrDefault(false);
    }

    private static bool AcceptsRule(string[] rule)
    {
        if (rule == null || rule.Length == 0)
            return true;

        var context = RunspaceContext.CurrentThread;

        var stringComparer = context.LanguageScope.GetBindingComparer();
        var resourceIdComparer = ResourceIdEqualityComparer.Default;

        var ruleRecord = context.RuleRecord;
        var ruleName = ruleRecord.RuleName;
        var ruleId = ruleRecord.RuleId;

        for (var i = 0; i < rule.Length; i++)
        {
            if (stringComparer.Equals(ruleName, rule[i]) || resourceIdComparer.Equals(ruleId, rule[i]))
                return true;
        }
        return false;
    }
}
