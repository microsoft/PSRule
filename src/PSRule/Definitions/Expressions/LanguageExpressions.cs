// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions
{
    internal delegate bool LanguageExpressionFn(ExpressionContext context, ExpressionInfo info, object[] args, object o);

    internal delegate bool? LanguageExpressionOuterFn(ExpressionContext context, object o);

    internal enum LanguageExpressionType
    {
        Operator = 1,

        Condition = 2,

        Function = 3
    }

    internal sealed class ExpressionInfo
    {
        private readonly string _Path;

        public ExpressionInfo(string path)
        {
            _Path = path;
        }
    }

    internal sealed class LanguageExpressionFactory
    {
        private readonly Dictionary<string, ILanguageExpresssionDescriptor> _Descriptors;

        public LanguageExpressionFactory()
        {
            _Descriptors = new Dictionary<string, ILanguageExpresssionDescriptor>(LanguageExpressions.Builtin.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var d in LanguageExpressions.Builtin)
                With(d);
        }

        public bool TryDescriptor(string name, out ILanguageExpresssionDescriptor descriptor)
        {
            descriptor = null;
            return !string.IsNullOrEmpty(name) &&
                _Descriptors.TryGetValue(name, out descriptor);
        }

        public bool IsSubselector(string name)
        {
            return name == "where";
        }

        public bool IsOperator(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Operator;
        }

        public bool IsCondition(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Condition;
        }

        public bool IsFunction(string name)
        {
            return TryDescriptor(name, out var d) &&
                d != null && d.Type == LanguageExpressionType.Function;
        }

        private void With(ILanguageExpresssionDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.Name, descriptor);
        }
    }

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
            return (context, o) => expression.Descriptor.Fn(context, info, new object[] { expression.Property }, o);
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
        private static string Value<T>(ExpressionContext context, object v)
        {
            return v as string;
        }

        private LanguageExpressionOuterFn Debugger(LanguageExpressionOuterFn expression, string path)
        {
            return !_Debugger ? expression : ((context, o) => DebuggerFn(context, path, expression, o));
        }

        private static bool? DebuggerFn(ExpressionContext context, string path, LanguageExpressionOuterFn expression, object o)
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

            var comparer = RunspaceContext.CurrentThread.LanguageScope.Binding.GetComparer();
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

        private static bool AcceptsSubselector(ExpressionContext context, LanguageExpressionOuterFn subselector, object o)
        {
            return subselector == null || subselector.Invoke(context, o).GetValueOrDefault(false);
        }

        private static bool AcceptsRule(string[] rule)
        {
            if (rule == null || rule.Length == 0)
                return true;

            var context = RunspaceContext.CurrentThread;

            var stringComparer = context.LanguageScope.Binding.GetComparer();
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

    /// <summary>
    /// Expressions that can be used with selectors.
    /// </summary>
    internal sealed class LanguageExpressions
    {
        // Conditions
        private const string EXISTS = "exists";
        private const string EQUALS = "equals";
        private const string NOTEQUALS = "notEquals";
        private const string HASDEFAULT = "hasDefault";
        private const string HASSCHEMA = "hasSchema";
        private const string HASVALUE = "hasValue";
        private const string MATCH = "match";
        private const string NOTMATCH = "notMatch";
        private const string IN = "in";
        private const string NOTIN = "notIn";
        private const string LESS = "less";
        private const string LESSOREQUALS = "lessOrEquals";
        private const string GREATER = "greater";
        private const string GREATEROREQUALS = "greaterOrEquals";
        private const string STARTSWITH = "startsWith";
        private const string NOTSTARTSWITH = "notStartsWith";
        private const string ENDSWITH = "endsWith";
        private const string NOTENDSWITH = "notEndsWith";
        private const string CONTAINS = "contains";
        private const string NOTCONTAINS = "notContains";
        private const string ISSTRING = "isString";
        private const string ISARRAY = "isArray";
        private const string ISBOOLEAN = "isBoolean";
        private const string ISDATETIME = "isDateTime";
        private const string ISINTEGER = "isInteger";
        private const string ISNUMERIC = "IsNumeric";
        private const string ISLOWER = "isLower";
        private const string ISUPPER = "isUpper";
        private const string SETOF = "setOf";
        private const string SUBSET = "subset";
        private const string COUNT = "count";
        private const string NOTCOUNT = "notCount";
        private const string VERSION = "version";
        private const string APIVERSION = "apiVersion";
        private const string WITHINPATH = "withinPath";
        private const string NOTWITHINPATH = "notWithinPath";
        private const string LIKE = "like";
        private const string NOTLIKE = "notLike";

        // Operators
        private const string IF = "if";
        private const string ANYOF = "anyOf";
        private const string ALLOF = "allOf";
        private const string NOT = "not";

        // Properties
        private const string FIELD = "field";
        private const string TYPE = "type";
        private const string NAME = "name";
        private const string CASESENSITIVE = "caseSensitive";
        private const string UNIQUE = "unique";
        private const string CONVERT = "convert";
        private const string IGNORESCHEME = "ignoreScheme";
        private const string INCLUDEPRERELEASE = "includePrerelease";
        private const string PROPERTY_SCHEMA = "$schema";
        private const string SOURCE = "source";
        private const string VALUE = "value";
        private const string SCOPE = "scope";

        // Comparisons
        private const string LESS_THAN = "<";
        private const string LESS_THAN_EQUALS = "<=";
        private const string GREATER_THAN = ">=";
        private const string GREATER_THAN_EQUALS = ">=";
        private const string DOT = ".";

        // Define built-ins
        internal static readonly ILanguageExpresssionDescriptor[] Builtin = new ILanguageExpresssionDescriptor[]
        {
            // Operators
            new LanguageExpresssionDescriptor(IF, LanguageExpressionType.Operator, If),
            new LanguageExpresssionDescriptor(ANYOF, LanguageExpressionType.Operator, AnyOf),
            new LanguageExpresssionDescriptor(ALLOF, LanguageExpressionType.Operator, AllOf),
            new LanguageExpresssionDescriptor(NOT, LanguageExpressionType.Operator, Not),

            // Conditions
            new LanguageExpresssionDescriptor(EXISTS, LanguageExpressionType.Condition, Exists),
            new LanguageExpresssionDescriptor(EQUALS, LanguageExpressionType.Condition, Equals),
            new LanguageExpresssionDescriptor(NOTEQUALS, LanguageExpressionType.Condition, NotEquals),
            new LanguageExpresssionDescriptor(HASVALUE, LanguageExpressionType.Condition, HasValue),
            new LanguageExpresssionDescriptor(MATCH, LanguageExpressionType.Condition, Match),
            new LanguageExpresssionDescriptor(NOTMATCH, LanguageExpressionType.Condition, NotMatch),
            new LanguageExpresssionDescriptor(IN, LanguageExpressionType.Condition, In),
            new LanguageExpresssionDescriptor(NOTIN, LanguageExpressionType.Condition, NotIn),
            new LanguageExpresssionDescriptor(LESS, LanguageExpressionType.Condition, Less),
            new LanguageExpresssionDescriptor(LESSOREQUALS, LanguageExpressionType.Condition, LessOrEquals),
            new LanguageExpresssionDescriptor(GREATER, LanguageExpressionType.Condition, Greater),
            new LanguageExpresssionDescriptor(GREATEROREQUALS, LanguageExpressionType.Condition, GreaterOrEquals),
            new LanguageExpresssionDescriptor(STARTSWITH, LanguageExpressionType.Condition, StartsWith),
            new LanguageExpresssionDescriptor(NOTSTARTSWITH, LanguageExpressionType.Condition, NotStartsWith),
            new LanguageExpresssionDescriptor(ENDSWITH, LanguageExpressionType.Condition, EndsWith),
            new LanguageExpresssionDescriptor(NOTENDSWITH, LanguageExpressionType.Condition, NotEndsWith),
            new LanguageExpresssionDescriptor(CONTAINS, LanguageExpressionType.Condition, Contains),
            new LanguageExpresssionDescriptor(NOTCONTAINS, LanguageExpressionType.Condition, NotContains),
            new LanguageExpresssionDescriptor(ISSTRING, LanguageExpressionType.Condition, IsString),
            new LanguageExpresssionDescriptor(ISARRAY, LanguageExpressionType.Condition, IsArray),
            new LanguageExpresssionDescriptor(ISBOOLEAN, LanguageExpressionType.Condition, IsBoolean),
            new LanguageExpresssionDescriptor(ISDATETIME, LanguageExpressionType.Condition, IsDateTime),
            new LanguageExpresssionDescriptor(ISINTEGER, LanguageExpressionType.Condition, IsInteger),
            new LanguageExpresssionDescriptor(ISNUMERIC, LanguageExpressionType.Condition, IsNumeric),
            new LanguageExpresssionDescriptor(ISLOWER, LanguageExpressionType.Condition, IsLower),
            new LanguageExpresssionDescriptor(ISUPPER, LanguageExpressionType.Condition, IsUpper),
            new LanguageExpresssionDescriptor(SETOF, LanguageExpressionType.Condition, SetOf),
            new LanguageExpresssionDescriptor(SUBSET, LanguageExpressionType.Condition, Subset),
            new LanguageExpresssionDescriptor(COUNT, LanguageExpressionType.Condition, Count),
            new LanguageExpresssionDescriptor(NOTCOUNT, LanguageExpressionType.Condition, NotCount),
            new LanguageExpresssionDescriptor(HASSCHEMA, LanguageExpressionType.Condition, HasSchema),
            new LanguageExpresssionDescriptor(VERSION, LanguageExpressionType.Condition, Version),
            new LanguageExpresssionDescriptor(APIVERSION, LanguageExpressionType.Condition, APIVersion),
            new LanguageExpresssionDescriptor(HASDEFAULT, LanguageExpressionType.Condition, HasDefault),
            new LanguageExpresssionDescriptor(WITHINPATH, LanguageExpressionType.Condition, WithinPath),
            new LanguageExpresssionDescriptor(NOTWITHINPATH, LanguageExpressionType.Condition, NotWithinPath),
            new LanguageExpresssionDescriptor(LIKE, LanguageExpressionType.Condition, Like),
            new LanguageExpresssionDescriptor(NOTLIKE, LanguageExpressionType.Condition, NotLike),
        };

        #region Operators

        internal static bool If(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            return inner.Length > 0 && (inner[0](context, o) ?? true);
        }

        internal static bool AnyOf(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (inner[i](context, o) ?? false)
                    return true;
            }
            return false;
        }

        internal static bool AllOf(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (!inner[i](context, o) ?? true)
                    return false;
            }
            return true;
        }

        internal static bool Not(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            return inner.Length > 0 && (!inner[0](context, o) ?? false);
        }

        #endregion Operators

        #region Conditions

        internal static bool Exists(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, EXISTS, out var propertyValue) && TryField(properties, out var field))
            {
                context.ExpressionTrace(EXISTS, field, propertyValue);
                return Condition(
                    context,
                    field,
                    propertyValue == ExpressionHelpers.Exists(context, o, field, caseSensitive: false),
                    ReasonStrings.Exists,
                    field
                );
            }
            return Invalid(context, EXISTS);
        }

        internal static bool Equals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyAny(properties, EQUALS, out var propertyValue) ||
                !TryOperand(context, EQUALS, o, properties, out var operand) ||
                !GetConvert(properties, out var convert) ||
                !GetCaseSensitive(properties, out var caseSensitive))
                return Invalid(context, EQUALS);

            // int, string, bool
            return Condition(
                context,
                operand,
                ExpressionHelpers.Equal(Value(context, propertyValue), Value(context, operand), caseSensitive, convertExpected: true, convertActual: convert),
                ReasonStrings.Assert_IsSetTo,
                operand.Value
            );
        }

        internal static bool NotEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyAny(properties, NOTEQUALS, out var propertyValue))
                return Invalid(context, NOTEQUALS);

            if (TryFieldNotExists(context, o, properties))
                return PassPathNotFound(context, NOTEQUALS);

            if (!TryOperand(context, NOTEQUALS, o, properties, out var operand) ||
                !GetConvert(properties, out var convert) ||
                !GetCaseSensitive(properties, out var caseSensitive))
                return Invalid(context, NOTEQUALS);

            // int, string, bool
            return Condition(
                context,
                operand,
                !ExpressionHelpers.Equal(Value(context, propertyValue), Value(context, operand), caseSensitive, convertExpected: true, convertActual: convert),
                ReasonStrings.Assert_IsSetTo,
                operand.Value
            );
        }

        internal static bool HasDefault(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyAny(properties, HASDEFAULT, out var propertyValue))
                return Invalid(context, HASDEFAULT);

            GetCaseSensitive(properties, out var caseSensitive);
            if (TryFieldNotExists(context, o, properties))
                return PassPathNotFound(context, HASDEFAULT);

            if (!TryOperand(context, HASDEFAULT, o, properties, out var operand))
                return Invalid(context, HASDEFAULT);

            return Condition(
                context,
                operand,
                ExpressionHelpers.Equal(propertyValue, operand.Value, caseSensitive, convertExpected: true),
                ReasonStrings.Assert_IsSetTo,
                operand.Value
            );
        }

        internal static bool HasValue(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyBool(properties, HASVALUE, out var propertyValue))
                return Invalid(context, HASVALUE);

            if (TryFieldNotExists(context, o, properties) && !propertyValue.Value)
                return PassPathNotFound(context, HASVALUE);

            if (!TryOperand(context, HASVALUE, o, properties, out var operand))
                return Invalid(context, HASVALUE);

            return Condition(
                context,
                operand,
                !propertyValue.Value == ExpressionHelpers.NullOrEmpty(operand.Value),
                ReasonStrings.Assert_IsSetTo,
                operand.Value
            );
        }

        internal static bool Match(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyAny(properties, MATCH, out var propertyValue) ||
                !TryOperand(context, MATCH, o, properties, out var operand) ||
                !GetCaseSensitive(properties, out var caseSensitive))
                return Invalid(context, MATCH);

            return Condition(
                    context,
                    operand,
                    ExpressionHelpers.Match(propertyValue, operand.Value, caseSensitive),
                    ReasonStrings.Assert_DoesNotMatch,
                    operand.Value,
                    propertyValue
                );
        }

        internal static bool NotMatch(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyAny(properties, NOTMATCH, out var propertyValue))
                return Invalid(context, NOTMATCH);

            if (TryFieldNotExists(context, o, properties))
                return PassPathNotFound(context, NOTMATCH);

            if (!TryOperand(context, NOTMATCH, o, properties, out var operand) ||
                !GetCaseSensitive(properties, out var caseSensitive))
                return Invalid(context, NOTMATCH);

            return Condition(
                context,
                operand,
                !ExpressionHelpers.Match(propertyValue, operand.Value, caseSensitive),
                ReasonStrings.Assert_Matches,
                operand.Value,
                propertyValue
            );
        }

        internal static bool In(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyArray(properties, IN, out var propertyValue) || !TryOperand(context, IN, o, properties, out var operand))
                return Invalid(context, IN);

            for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                if (ExpressionHelpers.AnyValue(operand.Value, propertyValue.GetValue(i), caseSensitive: false, out _))
                    return Pass();

            return Fail(
                context,
                operand,
                ReasonStrings.Assert_NotInSet,
                operand.Value,
                StringJoin(propertyValue)
            );
        }

        internal static bool NotIn(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyArray(properties, NOTIN, out var propertyValue))
                return Invalid(context, NOTIN);

            if (TryFieldNotExists(context, o, properties))
                return PassPathNotFound(context, NOTIN);

            if (!TryOperand(context, NOTIN, o, properties, out var operand))
                return Invalid(context, NOTIN);

            for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                if (ExpressionHelpers.AnyValue(operand.Value, propertyValue.GetValue(i), caseSensitive: false, out _))
                    return Fail(
                        context,
                        operand,
                        ReasonStrings.Assert_IsSetTo,
                        operand.Value
                    );

            return Pass();
        }

        internal static bool SetOf(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, SETOF, out var expectedValue) &&
                TryField(properties, out var field) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(SETOF, field, expectedValue);
                if (!ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object actualValue))
                    return NotHasField(context, field);

                if (!ExpressionHelpers.TryEnumerableLength(actualValue, out var count))
                    return Fail(context, field, ReasonStrings.NotEnumerable, field);

                if (count != expectedValue.Length)
                    return Fail(context, field, ReasonStrings.Count, field, count, expectedValue.Length);

                for (var i = 0; expectedValue != null && i < expectedValue.Length; i++)
                {
                    if (!ExpressionHelpers.AnyValue(actualValue, expectedValue.GetValue(i), caseSensitive, out _))
                        return Fail(context, field, ReasonStrings.Subset, field, expectedValue.GetValue(i));
                }
                return Pass();
            }
            return Invalid(context, SETOF);
        }

        internal static bool Subset(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, SUBSET, out var expectedValue) && TryField(properties, out var field) &&
                GetCaseSensitive(properties, out var caseSensitive) && GetUnique(properties, out var unique))
            {
                context.ExpressionTrace(SUBSET, field, expectedValue);
                if (!ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object actualValue))
                    return NotHasField(context, field);

                if (!ExpressionHelpers.TryEnumerableLength(actualValue, out _))
                    return Fail(context, field, ReasonStrings.NotEnumerable, field);

                for (var i = 0; expectedValue != null && i < expectedValue.Length; i++)
                {
                    if (!ExpressionHelpers.CountValue(actualValue, expectedValue.GetValue(i), caseSensitive, out var count) || (count > 1 && unique))
                        return count == 0
                            ? Fail(context, field, ReasonStrings.Subset, field, expectedValue.GetValue(i))
                            : Fail(context, field, ReasonStrings.SubsetDuplicate, field, expectedValue.GetValue(i));
                }
                return Pass();
            }
            return Invalid(context, SUBSET);
        }

        internal static bool Count(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, COUNT, out var expectedValue) ||
                !TryOperand(context, COUNT, o, properties, out var operand))
                return Invalid(context, COUNT);

            var operandValue = Value(context, operand);
            if (operandValue == null)
                return Fail(context, operand, ReasonStrings.Assert_IsNull);

            // int, string, bool
            return Condition(
                context,
                operand,
                ExpressionHelpers.TryEnumerableLength(operandValue, value: out var actualValue) && actualValue == expectedValue,
                ReasonStrings.Assert_Count,
                actualValue,
                expectedValue
            );
        }

        internal static bool NotCount(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, NOTCOUNT, out var expectedValue) ||
                !TryOperand(context, NOTCOUNT, o, properties, out var operand))
                return Invalid(context, NOTCOUNT);

            var operandValue = Value(context, operand);
            if (operandValue == null)
                return Fail(context, operand, ReasonStrings.Assert_IsNull);

            // int, string, bool
            return Condition(
                context,
                operand,
                ExpressionHelpers.TryEnumerableLength(operandValue, value: out var actualValue) && actualValue != expectedValue,
                ReasonStrings.Assert_NotCount,
                actualValue
            );
        }

        internal static bool Less(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, LESS, out var propertyValue) ||
                !TryOperand(context, LESS, o, properties, out var operand) ||
                !GetConvert(properties, out var convert))
                return Invalid(context, LESS);

            if (operand.Value == null)
                return Condition(
                    context,
                    operand,
                    0 < propertyValue,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            var operandValue = Value(context, operand);
            if (!ExpressionHelpers.CompareNumeric(
                operandValue,
                propertyValue,
                convert,
                compare: out var compare,
                value: out _))
                return Invalid(context, LESS);

            // int, string, bool
            return Condition(
                context,
                operand,
                compare < 0,
                ReasonStrings.Assert_NotComparedTo,
                operandValue,
                LESS_THAN,
                propertyValue
            );
        }

        internal static bool LessOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, LESSOREQUALS, out var propertyValue) ||
                !TryOperand(context, LESSOREQUALS, o, properties, out var operand) ||
                !GetConvert(properties, out var convert))
                return Invalid(context, LESSOREQUALS);

            if (operand.Value == null)
                return Condition(
                    context,
                    operand,
                    0 <= propertyValue,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            var operandValue = Value(context, operand);
            if (!ExpressionHelpers.CompareNumeric(
                operandValue,
                propertyValue,
                convert,
                compare: out var compare,
                value: out _))
                return Invalid(context, LESSOREQUALS);

            // int, string, bool
            return Condition(
                context,
                operand,
                compare <= 0,
                ReasonStrings.Assert_NotComparedTo,
                operandValue,
                LESS_THAN_EQUALS,
                propertyValue
            );
        }

        internal static bool Greater(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, GREATER, out var propertyValue) ||
                !TryOperand(context, GREATER, o, properties, out var operand) ||
                !GetConvert(properties, out var convert))
                return Invalid(context, GREATER);

            if (operand.Value == null)
                return Condition(
                    context,
                    operand,
                    0 > propertyValue,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            var operandValue = Value(context, operand);
            if (!ExpressionHelpers.CompareNumeric(
                operandValue,
                propertyValue,
                convert,
                compare: out var compare,
                value: out _))
                return Invalid(context, GREATER);

            // int, string, bool
            return Condition(
                context,
                operand,
                compare > 0,
                ReasonStrings.Assert_NotComparedTo,
                operandValue,
                GREATER_THAN,
                propertyValue
            );
        }

        internal static bool GreaterOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(context, properties, GREATEROREQUALS, out var propertyValue) ||
                !TryOperand(context, GREATEROREQUALS, o, properties, out var operand) ||
                !GetConvert(properties, out var convert))
                return Invalid(context, GREATEROREQUALS);

            if (operand.Value == null)
                return Condition(
                    context,
                    operand,
                    0 >= propertyValue,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            var operandValue = Value(context, operand);
            if (!ExpressionHelpers.CompareNumeric(
                operandValue,
                propertyValue,
                convert,
                compare: out var compare,
                value: out _))
                return Invalid(context, GREATEROREQUALS);

            // int, string, bool
            return Condition(
                context,
                operand,
                compare >= 0,
                ReasonStrings.Assert_NotComparedTo,
                operandValue,
                GREATER_THAN_EQUALS,
                propertyValue
            );
        }

        internal static bool StartsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, STARTSWITH, out var propertyValue) &&
                TryOperand(context, STARTSWITH, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(STARTSWITH, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                    return NotString(context, operand);

                for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                {
                    for (var i_value = 0; i_value < value.Length; i_value++)
                    {
                        if (ExpressionHelpers.StartsWith(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                            return Pass();
                    }
                }
                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotStartsWith,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return Invalid(context, STARTSWITH);
        }

        internal static bool NotStartsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, NOTSTARTSWITH, out var propertyValue) &&
                TryOperand(context, NOTSTARTSWITH, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(NOTSTARTSWITH, operand.Value, propertyValue);
                if (ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                {
                    for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                    {
                        for (var i_value = 0; i_value < value.Length; i_value++)
                        {
                            if (ExpressionHelpers.StartsWith(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                                return Fail(
                                    context,
                                    operand,
                                    ReasonStrings.Assert_StartsWith,
                                    value[i_value],
                                    propertyValue[i_propertyValue]
                                );
                        }
                    }
                }
                return Pass();
            }
            return Invalid(context, NOTSTARTSWITH);
        }

        internal static bool EndsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, ENDSWITH, out var propertyValue) &&
                TryOperand(context, ENDSWITH, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(ENDSWITH, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                    return NotString(context, operand);

                for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                {
                    for (var i_value = 0; i_value < value.Length; i_value++)
                    {
                        if (ExpressionHelpers.EndsWith(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                            return Pass();
                    }
                }
                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotEndsWith,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return Invalid(context, ENDSWITH);
        }

        internal static bool NotEndsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, NOTENDSWITH, out var propertyValue) &&
                TryOperand(context, NOTENDSWITH, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(NOTENDSWITH, operand.Value, propertyValue);
                if (ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                {
                    for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                    {
                        for (var i_value = 0; i_value < value.Length; i_value++)
                        {
                            if (ExpressionHelpers.EndsWith(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                                return Fail(
                                    context,
                                    operand,
                                    ReasonStrings.Assert_EndsWith,
                                    value[i_value],
                                    propertyValue[i_propertyValue]
                                );
                        }
                    }
                }
                return Pass();
            }
            return Invalid(context, NOTENDSWITH);
        }

        internal static bool Contains(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, CONTAINS, out var propertyValue) &&
                TryOperand(context, CONTAINS, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(CONTAINS, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                    return NotString(context, operand);

                for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                {
                    for (var i_value = 0; i_value < value.Length; i_value++)
                    {
                        if (ExpressionHelpers.Contains(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                            return Pass();
                    }
                }
                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotContains,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return Invalid(context, CONTAINS);
        }

        internal static bool NotContains(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, NOTCONTAINS, out var propertyValue) &&
                TryOperand(context, NOTCONTAINS, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(NOTCONTAINS, operand.Value, propertyValue);
                if (ExpressionHelpers.TryStringOrArray(operand.Value, convert, out var value))
                {
                    for (var i_propertyValue = 0; propertyValue != null && i_propertyValue < propertyValue.Length; i_propertyValue++)
                    {
                        for (var i_value = 0; i_value < value.Length; i_value++)
                        {
                            if (ExpressionHelpers.Contains(value[i_value], propertyValue[i_propertyValue], caseSensitive))
                                return Fail(
                                    context,
                                    operand,
                                    ReasonStrings.Assert_Contains,
                                    value[i_value],
                                    propertyValue[i_propertyValue]
                                );
                        }
                    }
                }
                return Pass();
            }
            return Invalid(context, NOTCONTAINS);
        }

        internal static bool IsString(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISSTRING, out var propertyValue) &&
                TryOperand(context, ISSTRING, o, properties, out var operand))
            {
                context.ExpressionTrace(ISSTRING, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.TryString(operand.Value, out _),
                    ReasonStrings.Assert_NotString,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsArray(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISARRAY, out var propertyValue) &&
                TryOperand(context, ISARRAY, o, properties, out var operand))
            {
                context.ExpressionTrace(ISARRAY, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.TryArray(operand.Value, out _),
                    ReasonStrings.Assert_NotArray,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsBoolean(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISBOOLEAN, out var propertyValue) &&
                TryOperand(context, ISBOOLEAN, o, properties, out var operand) &&
                GetConvert(properties, out var convert))
            {
                context.ExpressionTrace(ISBOOLEAN, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.TryBool(operand.Value, convert, out _),
                    ReasonStrings.Assert_NotBoolean,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsDateTime(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISDATETIME, out var propertyValue) &&
                TryOperand(context, ISDATETIME, o, properties, out var operand) &&
                GetConvert(properties, out var convert))
            {
                context.ExpressionTrace(ISDATETIME, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.TryDateTime(operand.Value, convert, out _),
                    ReasonStrings.Assert_NotDateTime,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsInteger(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISINTEGER, out var propertyValue) &&
                TryOperand(context, ISINTEGER, o, properties, out var operand) &&
                GetConvert(properties, out var convert))
            {
                context.ExpressionTrace(ISINTEGER, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == (ExpressionHelpers.TryInt(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryLong(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryByte(operand.Value, convert, out _)),
                    ReasonStrings.Assert_NotInteger,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsNumeric(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISNUMERIC, out var propertyValue) &&
                TryOperand(context, ISNUMERIC, o, properties, out var operand) &&
                GetConvert(properties, out var convert))
            {
                context.ExpressionTrace(ISNUMERIC, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == (ExpressionHelpers.TryInt(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryLong(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryFloat(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryByte(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryDouble(operand.Value, convert, out _)),
                    ReasonStrings.Assert_NotInteger,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool WithinPath(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryOperand(context, WITHINPATH, o, properties, out var operand) &&
                TryPropertyStringArray(properties, WITHINPATH, out var path) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(WITHINPATH, operand.Value, path);

                if (ExpressionHelpers.TryString(operand.Value, out var source))
                {
                    for (var i = 0; path != null && i < path.Length; i++)
                    {
                        if (ExpressionHelpers.WithinPath(source, path[i], caseSensitive))
                            return Pass();
                    }
                }
                return Fail(
                    context,
                    operand,
                    ReasonStrings.WithinPath,
                    ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), source),
                    StringJoinNormalizedPath(path)
                );
            }

            return Invalid(context, WITHINPATH);
        }

        internal static bool NotWithinPath(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryOperand(context, NOTWITHINPATH, o, properties, out var operand) &&
                TryPropertyStringArray(properties, NOTWITHINPATH, out var path) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(NOTWITHINPATH, operand.Value, path);

                if (ExpressionHelpers.TryString(operand.Value, out var source))
                {
                    for (var i = 0; path != null && i < path.Length; i++)
                    {
                        if (ExpressionHelpers.WithinPath(source, path[i], caseSensitive))
                            return Fail(
                                context,
                                operand,
                                ReasonStrings.NotWithinPath,
                                ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), source),
                                ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), path[i])
                            );
                    }
                }
                return Pass();
            }

            return Invalid(context, NOTWITHINPATH);
        }

        internal static bool Like(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, LIKE, out var propertyValue) &&
                TryOperand(context, LIKE, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(LIKE, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryString(operand.Value, convert, out var value))
                    return NotString(context, operand);

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                    if (ExpressionHelpers.Like(value, propertyValue[i], caseSensitive))
                        return Pass();

                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotLike,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return Invalid(context, LIKE);
        }

        internal static bool NotLike(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, NOTLIKE, out var propertyValue) &&
                TryOperand(context, NOTLIKE, o, properties, out var operand) &&
                GetConvert(properties, out var convert) &&
                GetCaseSensitive(properties, out var caseSensitive))
            {
                context.ExpressionTrace(NOTLIKE, operand.Value, propertyValue);
                if (ExpressionHelpers.TryString(operand.Value, convert, out var value))
                {
                    for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                    {
                        if (ExpressionHelpers.Like(value, propertyValue[i], caseSensitive))
                            return Fail(
                                context,
                                operand,
                                ReasonStrings.Assert_Like,
                                value,
                                propertyValue[i]
                            );
                    }
                }
                return Pass();
            }
            return Invalid(context, NOTLIKE);
        }

        internal static bool IsLower(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISLOWER, out var propertyValue) &&
                TryOperand(context, ISLOWER, o, properties, out var operand))
            {
                if (!ExpressionHelpers.TryString(operand.Value, out var value))
                    return Condition(
                        context,
                        operand,
                        !propertyValue.Value,
                        ReasonStrings.Assert_NotString,
                        operand.Value
                    );

                context.ExpressionTrace(ISLOWER, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.IsLower(value, requireLetters: false, notLetter: out _),
                    ReasonStrings.Assert_IsLower,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool IsUpper(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISUPPER, out var propertyValue) &&
                TryOperand(context, ISUPPER, o, properties, out var operand))
            {
                if (!ExpressionHelpers.TryString(operand.Value, out var value))
                    return Condition(
                        context,
                        operand,
                        !propertyValue.Value,
                        ReasonStrings.Assert_NotString,
                        operand.Value
                    );

                context.ExpressionTrace(ISUPPER, operand.Value, propertyValue);
                return Condition(
                    context,
                    operand,
                    propertyValue == ExpressionHelpers.IsUpper(value, requireLetters: false, notLetter: out _),
                    ReasonStrings.Assert_IsUpper,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool HasSchema(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, HASSCHEMA, out var expectedValue) &&
                TryField(properties, out var field) &&
                TryPropertyBoolOrDefault(properties, IGNORESCHEME, out var ignoreScheme, false))
            {
                context.ExpressionTrace(HASSCHEMA, field, expectedValue);
                if (!ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object actualValue))
                    return NotHasField(context, field);

                if (!ObjectHelper.GetPath(context, actualValue, PROPERTY_SCHEMA, caseSensitive: false, out object schemaValue))
                    return NotHasField(context, PROPERTY_SCHEMA);

                if (!ExpressionHelpers.TryString(schemaValue, out var actualSchema))
                    return NotString(context, Operand.FromPath(PROPERTY_SCHEMA, schemaValue));

                if (string.IsNullOrEmpty(actualSchema))
                    return NullOrEmpty(context, Operand.FromPath(PROPERTY_SCHEMA, schemaValue));

                if (expectedValue == null || expectedValue.Length == 0)
                    return Pass();

                if (ExpressionHelpers.AnySchema(actualSchema, expectedValue, ignoreScheme, false))
                    return Pass();

                return Fail(
                    context,
                    field,
                    ReasonStrings.Assert_NotSpecifiedSchema,
                    actualSchema
                );
            }
            return Invalid(context, HASSCHEMA);
        }

        internal static bool Version(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyString(properties, VERSION, out var expectedValue) &&
                TryOperand(context, VERSION, o, properties, out var operand) &&
                TryPropertyBoolOrDefault(properties, INCLUDEPRERELEASE, out var includePrerelease, false))
            {
                context.ExpressionTrace(VERSION, operand.Value, expectedValue);
                if (!ExpressionHelpers.TryString(operand.Value, out var version))
                    return NotString(context, operand);

                if (!SemanticVersion.TryParseVersion(version, out var actualVersion))
                    return Fail(context, operand, ReasonStrings.Version, operand.Value);

                if (!SemanticVersion.TryParseConstraint(expectedValue, out var constraint, includePrerelease))
                    throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, expectedValue));

                if (constraint != null && !constraint.Equals(actualVersion))
                    return Fail(context, operand, ReasonStrings.VersionContraint, actualVersion, constraint);

                return Pass();
            }
            return Invalid(context, VERSION);
        }

        internal static bool APIVersion(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyString(properties, APIVERSION, out var expectedValue) &&
                TryOperand(context, APIVERSION, o, properties, out var operand) &&
                TryPropertyBoolOrDefault(properties, INCLUDEPRERELEASE, out var includePrerelease, false))
            {
                context.ExpressionTrace(APIVERSION, operand.Value, expectedValue);
                if (!ExpressionHelpers.TryString(operand.Value, out var version))
                    return NotString(context, operand);

                if (!DateVersion.TryParseVersion(version, out var actualVersion))
                    return Fail(context, operand, ReasonStrings.Version, operand.Value);

                if (!DateVersion.TryParseConstraint(expectedValue, out var constraint, includePrerelease))
                    throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, expectedValue));

                if (constraint != null && !constraint.Equals(actualVersion))
                    return Fail(context, operand, ReasonStrings.VersionContraint, actualVersion, constraint);

                return Pass();
            }
            return Invalid(context, APIVERSION);
        }

        #endregion Conditions

        #region Helper methods

        private static bool Condition(IExpressionContext context, string path, bool condition, string text, params object[] args)
        {
            if (condition)
                return true;

            context.Reason(Operand.FromPath(path, null), text, args);
            return false;
        }

        private static bool Condition(IExpressionContext context, IOperand operand, bool condition, string text, params object[] args)
        {
            if (condition)
                return true;

            context.Reason(operand, text, args);
            return false;
        }

        private static bool Fail(IExpressionContext context, string path, string text, params object[] args)
        {
            return Condition(context, Operand.FromPath(path, null), false, text, args);
        }

        private static bool Fail(IExpressionContext context, IOperand operand, string text, params object[] args)
        {
            return Condition(context, operand, false, text, args);
        }

        private static bool PassPathNotFound(ExpressionContext context, string name)
        {
            context.ExpressionTrace(name, PSRuleResources.ObjectPathNotFound);
            return true;
        }

        private static bool Pass()
        {
            return true;
        }

        private static bool Invalid(IExpressionContext context, string name)
        {
            return false;
        }

        /// <summary>
        /// Reason: The field '{0}' does not exist.
        /// </summary>
        private static bool NotHasField(IExpressionContext context, string path)
        {
            return Fail(context, path, ReasonStrings.NotHasField, path);
        }

        /// <summary>
        /// Reason: Is null or empty.
        /// </summary>
        private static bool NullOrEmpty(ExpressionContext context, IOperand operand)
        {
            return Fail(context, operand, ReasonStrings.Assert_IsNullOrEmpty);
        }

        /// <summary>
        /// Reason: The value '{0}' is not a string.
        /// </summary>
        private static bool NotString(ExpressionContext context, IOperand operand)
        {
            return Fail(context, operand, ReasonStrings.Assert_NotString, operand.Value);
        }

        private static bool TryPropertyAny(LanguageExpression.PropertyBag properties, string propertyName, out object propertyValue)
        {
            return properties.TryGetValue(propertyName, out propertyValue);
        }

        private static bool TryPropertyString(LanguageExpression.PropertyBag properties, string propertyName, out string propertyValue)
        {
            return properties.TryGetString(propertyName, out propertyValue);
        }

        private static bool TryPropertyBool(LanguageExpression.PropertyBag properties, string propertyName, out bool? propertyValue)
        {
            return properties.TryGetBool(propertyName, out propertyValue);
        }

        private static bool TryPropertyBoolOrDefault(LanguageExpression.PropertyBag properties, string propertyName, out bool propertyValue, bool defaultValue)
        {
            propertyValue = defaultValue;
            if (properties.TryGetBool(propertyName, out var value))
                propertyValue = value.Value;

            return true;
        }

        private static bool TryPropertyLong(ExpressionContext context, LanguageExpression.PropertyBag properties, string propertyName, out long? propertyValue)
        {
            propertyValue = null;
            if (!properties.TryGetValue(propertyName, out var value))
                return false;

            if (!ExpressionHelpers.TryLong(Value(context, value), true, out var l_value))
                return false;

            propertyValue = l_value;
            return true;
        }

        private static bool TryField(LanguageExpression.PropertyBag properties, out string field)
        {
            return properties.TryGetString(FIELD, out field);
        }

        private static bool TryField(IExpressionContext context, LanguageExpression.PropertyBag properties, object o, out IOperand operand)
        {
            operand = null;
            if (!properties.TryGetString(FIELD, out var field))
                return false;

            if (ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object value))
                operand = Operand.FromPath(field, value);

            return operand != null || NotHasField(context, field);
        }

        private static bool TryName(IExpressionContext context, LanguageExpression.PropertyBag properties, object o, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(NAME, out var svalue))
            {
                if (svalue != DOT || context?.Context?.LanguageScope == null)
                    return Invalid(context, svalue);

                if (!context.Context.LanguageScope.TryGetName(o, out var name, out var path) ||
                    string.IsNullOrEmpty(name))
                    return Invalid(context, svalue);

                operand = Operand.FromName(name, path);
            }
            return operand != null;
        }

        private static bool TryType(IExpressionContext context, LanguageExpression.PropertyBag properties, object o, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(TYPE, out var svalue))
            {
                if (svalue != DOT || context?.Context?.LanguageScope == null)
                    return Invalid(context, svalue);

                if (!context.Context.LanguageScope.TryGetType(o, out var type, out var path) ||
                    string.IsNullOrEmpty(type))
                    return Invalid(context, svalue);

                operand = Operand.FromType(type, path);
            }
            return operand != null;
        }

        private static bool TrySource(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(SOURCE, out var sourceValue))
            {
                var source = context?.Context?.TargetObject?.Source[sourceValue];
                if (source == null)
                    return Invalid(context, sourceValue);

                operand = Operand.FromSource(ExpressionHelpers.GetObjectOriginPath(source));
            }
            return operand != null;
        }

        private static bool TryValue(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetValue(VALUE, out var value))
            {
                // TODO: Propogate path
                operand = Operand.FromValue(value);
            }
            return operand != null;
        }

        private static bool TryScope(IExpressionContext context, LanguageExpression.PropertyBag properties, object o, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(SCOPE, out var svalue))
            {
                if (svalue != DOT || context?.Context?.LanguageScope == null)
                    return Invalid(context, svalue);

                if (!context.Context.LanguageScope.TryGetScope(o, out var scope))
                    return Invalid(context, svalue);

                operand = Operand.FromScope(scope);
            }
            return operand != null;
        }

        /// <summary>
        /// Unwrap a function delegate or a literal value.
        /// </summary>
        private static object Value(IExpressionContext context, IOperand operand)
        {
            if (operand == null)
                return null;

            return operand.Value is ExpressionFnOuter fn ? fn(context) : operand.Value;
        }

        /// <summary>
        /// Unwrap a function delegate or a literal value.
        /// </summary>
        private static object Value(IExpressionContext context, object value)
        {
            return value is ExpressionFnOuter fn ? fn(context) : value;
        }

        private static bool GetCaseSensitive(LanguageExpression.PropertyBag properties, out bool caseSensitive, bool defaultValue = false)
        {
            return TryPropertyBoolOrDefault(properties, CASESENSITIVE, out caseSensitive, defaultValue);
        }

        private static bool GetUnique(LanguageExpression.PropertyBag properties, out bool unique, bool defaultValue = false)
        {
            return TryPropertyBoolOrDefault(properties, UNIQUE, out unique, defaultValue);
        }

        private static bool GetConvert(LanguageExpression.PropertyBag properties, out bool convert, bool defaultValue = false)
        {
            return TryPropertyBoolOrDefault(properties, CONVERT, out convert, defaultValue);
        }

        /// <summary>
        /// Returns true when the field properties is specified and the specified field does not exist.
        /// </summary>
        private static bool TryFieldNotExists(ExpressionContext context, object o, LanguageExpression.PropertyBag properties)
        {
            return properties.TryGetString(FIELD, out var field) &&
                !ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object _);
        }

        private static bool TryOperand(ExpressionContext context, string name, object o, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            return TryField(context, properties, o, out operand) ||
                TryType(context, properties, o, out operand) ||
                TryName(context, properties, o, out operand) ||
                TrySource(context, properties, out operand) ||
                TryValue(context, properties, out operand) ||
                TryScope(context, properties, o, out operand) ||
                Invalid(context, name);
        }

        private static bool TryPropertyArray(LanguageExpression.PropertyBag properties, string propertyName, out Array propertyValue)
        {
            if (properties.TryGetValue(propertyName, out var array) && array is Array arrayValue)
            {
                propertyValue = arrayValue;
                return true;
            }
            propertyValue = null;
            return false;
        }

        private static bool TryPropertyStringArray(LanguageExpression.PropertyBag properties, string propertyName, out string[] propertyValue)
        {
            if (properties.TryGetStringArray(propertyName, out propertyValue))
            {
                return true;
            }
            else if (properties.TryGetString(propertyName, out var s))
            {
                propertyValue = new string[] { s };
                return true;
            }
            propertyValue = null;
            return false;
        }

        private static LanguageExpression.PropertyBag GetProperties(object[] args)
        {
            return (LanguageExpression.PropertyBag)args[0];
        }

        private static LanguageExpressionOuterFn[] GetInner(object[] args)
        {
            return (LanguageExpressionOuterFn[])args;
        }

        private static string StringJoin(Array propertyValue)
        {
            return string.Concat("'", string.Join("', '", propertyValue), "'");
        }

        private static string StringJoinNormalizedPath(string[] path)
        {
            var normalizedPath = path.Select(p => ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), p));
            return StringJoin(normalizedPath.ToArray());
        }

        #endregion Helper methods
    }
}
