// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

internal sealed class LanguageExpressionBuilder(ResourceId id, bool debugger = true)
{
    private const char Dot = '.';
    private const char OpenBracket = '[';
    private const char CloseBracket = ']';

    private const string DOT_WHERE = ".where";
    private const string LESS = "less";
    private const string LESS_OR_EQUAL = "lessOrEqual";
    private const string GREATER = "greater";
    private const string GREATER_OR_EQUAL = "greaterOrEqual";
    private const string COUNT = "count";

    private readonly ResourceId _Id = id;
    private readonly bool _Debugger = debugger;

    private ResourceId[]? _With;
    private string[]? _Type;
    private LanguageExpression? _When;
    private ResourceId[]? _Rule;

    public LanguageExpressionBuilder WithSelector(string[] with)
    {
        if (with == null || with.Length == 0)
            return this;

        _With = GetRelatedResourceID(_Id.Scope, with);
        return this;
    }

    public LanguageExpressionBuilder WithType(string[]? type)
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

        _Rule = GetRelatedResourceID(_Id.Scope, rule);
        return this;
    }

    public LanguageExpressionOuterFn Build(LanguageIf? condition)
    {
        condition ??= new LanguageIf(LanguageExpressionLambda.True);
        return Precondition(Expression(string.Empty, condition.Expression), _With, _Type, Expression(string.Empty, _When), _Rule);
    }

    private static LanguageExpressionOuterFn Precondition(LanguageExpressionOuterFn expression, ResourceId[]? with, string[]? type, LanguageExpressionOuterFn? when, ResourceId[]? rule)
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

    private static LanguageExpressionOuterFn PreconditionRule(ResourceId[] rule, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            // Evaluate selector rule pre-condition
            if (!AcceptsRule(context, rule))
            {
                context.Logger.LogDebug(EventId.None, PSRuleResources.DebugTargetRuleMismatch);
                return null;
            }
            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn PreconditionSelector(ResourceId[] with, LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            // Evaluate selector pre-condition
            if (!AcceptsWith(context, with, o))
            {
                context.Logger.LogDebug(EventId.None, PSRuleResources.DebugTargetTypeMismatch);
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
            if (!AcceptsType(context, type))
            {
                context.Logger.LogDebug(EventId.None, PSRuleResources.DebugTargetTypeMismatch);
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
                    context.Logger.LogDebug(EventId.None, PSRuleResources.DebugTargetSubselectorMismatch);
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

    private LanguageExpressionOuterFn? Expression(string path, LanguageExpression? expression)
    {
        if (expression == null)
            return null;

        path = Path(path, expression);
        if (expression is LanguageOperator selectorOperator)
            return Scope(Debugger(Operator(path, selectorOperator), path));
        else if (expression is LanguageCondition selectorCondition)
            return Scope(Debugger(Condition(path, selectorCondition), path));
        else if (expression is LanguageExpressionLambda selectorLambda)
            return Scope(Debugger(Lambda(path, selectorLambda), path));

        throw new InvalidOperationException();
    }

    private static LanguageExpressionOuterFn Scope(LanguageExpressionOuterFn fn)
    {
        return (context, o) =>
        {
            LegacyRunspaceContext.CurrentThread?.EnterLanguageScope(context.Source);

            return fn(context, o);
        };
    }

    private static LanguageExpressionOuterFn Condition(string path, LanguageCondition expression)
    {
        var info = new ExpressionInfo(path);
        return (context, o) => expression.Descriptor.Fn(context, info, [expression.Property], o);
    }

    private static LanguageExpressionOuterFn Lambda(string path, LanguageExpressionLambda expression)
    {
        var info = new ExpressionInfo(path);
        return (context, o) => expression.Descriptor.Fn(context, info, null, o);
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
            var subselector = expression.Subselector != null ? Expression(string.Concat(path, DOT_WHERE), expression.Subselector) : null;
            return (context, o) =>
            {
                var objectPath = Value<string>(context, expression.Property["field"]);
                ObjectHelper.GetPath(
                    bindingContext: context,
                    targetObject: o.Value,
                    path: objectPath,
                    caseSensitive: false,
                    out object[] items
                );

                var quantifier = GetQuantifier(expression);
                var pass = 0;

                // If any fail, all fail
                for (var i = 0; items != null && i < items.Length; i++)
                {
                    var child = TargetObjectChildIndex(items[i], objectPath, i);
                    if (subselector == null || subselector(context, child).GetValueOrDefault(true))
                    {
                        if (!expression.Descriptor.Fn(context, info, innerA, child))
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

    private static TargetObjectChild TargetObjectChildIndex(object o, string path, int index)
    {
        path = string.Concat(path, OpenBracket, index, CloseBracket);
        return new TargetObjectChild(o, path);
    }

    /// <summary>
    /// Returns a quantifier function if set for the expression.
    /// </summary>
    private static Func<long, bool>? GetQuantifier(LanguageOperator expression)
    {
        if (expression.Property.TryGetLong(GREATER_OR_EQUAL, out var q) && q != null)
            return (number) => number >= q.Value;

        if (expression.Property.TryGetLong(GREATER, out q) && q != null)
            return (number) => number > q.Value;

        if (expression.Property.TryGetLong(LESS_OR_EQUAL, out q) && q != null)
            return (number) => number <= q.Value;

        if (expression.Property.TryGetLong(LESS, out q) && q != null)
            return (number) => number < q.Value;

        if (expression.Property.TryGetLong(COUNT, out q) && q != null)
            return (number) => number == q.Value;

        return null;
    }

    [DebuggerStepThrough]
    private static string? Value<T>(IExpressionContext context, object v)
    {
        return v as string;
    }

    private LanguageExpressionOuterFn Debugger(LanguageExpressionOuterFn expression, string path)
    {
        return !_Debugger ? expression : ((context, o) => DebuggerFn(context, path, expression, o));
    }

    private static bool? DebuggerFn(IExpressionContext context, string path, LanguageExpressionOuterFn expression, ITargetObject o)
    {
        var result = expression(context, o);
        var type = context.Kind == ResourceKind.Rule ? 'R' : 'S';
        context.Logger.LogDebug(EventId.None, PSRuleResources.LanguageExpressionTraceP2, type, path, result);
        return result;
    }

    private static bool AcceptsType(IExpressionContext context, string[] type)
    {
        if (type == null || type.Length == 0)
            return true;

        if (!context.Context.Scope.TryGetType(context.Current, out var targetType, out _))
            return false;

        var comparer = context.Context.Scope.GetBindingComparer();
        for (var i = 0; i < type.Length; i++)
        {
            if (comparer.Equals(targetType, type[i]))
            {
                return true;
            }
        }
        return false;
    }

    private static bool AcceptsWith(IExpressionContext context, ResourceId[] with, ITargetObject o)
    {
        if (with == null || with.Length == 0)
            return true;

        for (var i = 0; i < with.Length; i++)
        {
            if (context.TrySelector(with[i], o))
            {
                return true;
            }
        }
        return false;
    }

    private static bool AcceptsSubselector(IExpressionContext context, LanguageExpressionOuterFn subselector, ITargetObject o)
    {
        return subselector == null || subselector.Invoke(context, o).GetValueOrDefault(false);
    }

    private static bool AcceptsRule(IExpressionContext context, ResourceId[] rule)
    {
        if (context.RuleId == null)
            return false;

        if (rule == null || rule.Length == 0)
            return true;

        var stringComparer = context.Context.Scope.GetBindingComparer();
        var resourceIdComparer = ResourceIdEqualityComparer.Default;

        // Allow short name cases.
        var ruleName = context.RuleId.Value.Name;

        for (var i = 0; i < rule.Length; i++)
        {
            if (stringComparer.Equals(ruleName, rule[i]) || resourceIdComparer.Equals(context.RuleId.Value, rule[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Convert the identifiers to resource IDs.
    /// </summary>
    private static ResourceId[] GetRelatedResourceID(string defaultScope, string[] resourceNameOrId)
    {
        return ResourceHelper.GetResourceId(defaultScope, resourceNameOrId, ResourceIdKind.Unknown) ?? [];
    }
}
