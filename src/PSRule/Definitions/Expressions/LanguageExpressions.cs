// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
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

        Condition = 2
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
            return _Descriptors.TryGetValue(name, out descriptor);
        }

        public bool IsOperator(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Operator;
        }

        public bool IsCondition(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Condition;
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
        private const char CloseBracket = '[';

        private readonly bool _Debugger;

        private string[] _With;
        private string[] _Type;
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

        public LanguageExpressionBuilder WithRule(string[] rule)
        {
            if (rule == null || rule.Length == 0)
                return this;

            _Rule = rule;
            return this;
        }

        public LanguageExpressionOuterFn Build(LanguageIf selectorIf)
        {
            return Precondition(Expression(string.Empty, selectorIf.Expression), _With, _Type, _Rule);
        }

        private static LanguageExpressionOuterFn Precondition(LanguageExpressionOuterFn expression, string[] with, string[] type, string[] rule)
        {
            var fn = expression;
            if (type != null)
                fn = PreconditionType(type, fn);

            if (with != null)
                fn = PreconditionSelector(with, fn);

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
                // Evalute selector pre-condition
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
                // Evalute type pre-condition
                if (!AcceptsType(type))
                {
                    context.Debug(PSRuleResources.DebugTargetTypeMismatch);
                    return null;
                }
                return fn(context, o);
            };
        }

        private LanguageExpressionOuterFn Expression(string path, LanguageExpression expression)
        {
            path = Path(path, expression);
            if (expression is LanguageOperator selectorOperator)
                return Debugger(Operator(path, selectorOperator), path);
            else if (expression is LanguageCondition selectorCondition)
                return Debugger(Condition(path, selectorCondition), path);

            throw new InvalidOperationException();
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
            return (context, o) => expression.Descriptor.Fn(context, info, innerA, o);
        }

        private LanguageExpressionOuterFn Debugger(LanguageExpressionOuterFn expression, string path)
        {
            return !_Debugger ? expression : ((context, o) => DebuggerFn(context, path, expression, o));
        }

        private static bool? DebuggerFn(ExpressionContext context, string path, LanguageExpressionOuterFn expression, object o)
        {
            var result = expression(context, o);
            context.Debug(PSRuleResources.SelectorTrace, path, result);
            return result;
        }

        private static bool AcceptsType(string[] type)
        {
            if (type == null)
                return true;

            var comparer = RunspaceContext.CurrentThread.Pipeline.Baseline.GetTargetBinding().IgnoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

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

        private static bool AcceptsRule(string[] rule)
        {
            if (rule == null || rule.Length == 0)
                return true;

            var context = RunspaceContext.CurrentThread;

            var stringComparer = context.Pipeline.Baseline.GetTargetBinding().IgnoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

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
        private const string ENDSWITH = "endsWith";
        private const string CONTAINS = "contains";
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
        private const string VERSION = "version";

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

        // Comparisons
        private const string LESS_THAN = "<";
        private const string LESS_THAN_EQUALS = "<=";
        private const string GREATER_THAN = ">=";
        private const string GREATER_THAN_EQUALS = ">=";

        // Define built-ins
        internal readonly static ILanguageExpresssionDescriptor[] Builtin = new ILanguageExpresssionDescriptor[]
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
            new LanguageExpresssionDescriptor(ENDSWITH, LanguageExpressionType.Condition, EndsWith),
            new LanguageExpresssionDescriptor(CONTAINS, LanguageExpressionType.Condition, Contains),
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
            new LanguageExpresssionDescriptor(HASSCHEMA, LanguageExpressionType.Condition, HasSchema),
            new LanguageExpresssionDescriptor(VERSION, LanguageExpressionType.Condition, Version),
            new LanguageExpresssionDescriptor(HASDEFAULT, LanguageExpressionType.Condition, HasDefault),
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
            if (!TryPropertyAny(properties, EQUALS, out var propertyValue) || !TryOperand(context, EQUALS, o, properties, out var operand))
                return Invalid(context, EQUALS);

            // int, string, bool
            return Condition(
                context,
                ExpressionHelpers.Equal(propertyValue, operand.Value, caseSensitive: false, convertExpected: true),
                operand,
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
                return Pass();

            if (!TryOperand(context, NOTEQUALS, o, properties, out var operand))
                return Invalid(context, NOTEQUALS);

            // int, string, bool
            return Condition(
                context,
                !ExpressionHelpers.Equal(propertyValue, operand.Value, caseSensitive: false, convertExpected: true),
                operand,
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
                return Pass();

            if (!TryOperand(context, HASDEFAULT, o, properties, out var operand))
                return Invalid(context, HASDEFAULT);

            return Condition(
                context,
                ExpressionHelpers.Equal(propertyValue, operand.Value, caseSensitive, convertExpected: true),
                operand,
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
                return Pass();

            if (!TryOperand(context, HASVALUE, o, properties, out var operand))
                return Invalid(context, HASVALUE);

            return Condition(
                context,
                !propertyValue.Value == ExpressionHelpers.NullOrEmpty(operand.Value),
                operand,
                ReasonStrings.Assert_IsSetTo,
                operand.Value
            );
        }

        internal static bool Match(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            return !TryPropertyAny(properties, MATCH, out var propertyValue) || !TryOperand(context, MATCH, o, properties, out var operand)
                ? Invalid(context, MATCH)
                : Condition(
                    context,
                    ExpressionHelpers.Match(propertyValue, operand.Value, caseSensitive: false),
                    operand,
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
                return Pass();

            if (!TryOperand(context, NOTMATCH, o, properties, out var operand))
                return Invalid(context, NOTMATCH);

            return Condition(
                context,
                !ExpressionHelpers.Match(propertyValue, operand.Value, caseSensitive: false),
                operand,
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
                return Pass();

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
                    return Fail(context, ReasonStrings.NotEnumerable, field);

                if (count != expectedValue.Length)
                    return Fail(context, ReasonStrings.Count, field, count, expectedValue.Length);

                for (var i = 0; expectedValue != null && i < expectedValue.Length; i++)
                {
                    if (!ExpressionHelpers.AnyValue(actualValue, expectedValue.GetValue(i), caseSensitive, out _))
                        return Fail(context, ReasonStrings.Subset, field, expectedValue.GetValue(i));
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
                    return Fail(context, ReasonStrings.NotEnumerable, field);

                for (var i = 0; expectedValue != null && i < expectedValue.Length; i++)
                {
                    if (!ExpressionHelpers.CountValue(actualValue, expectedValue.GetValue(i), caseSensitive, out var count) || (count > 1 && unique))
                        return count == 0
                            ? Fail(context, ReasonStrings.Subset, field, expectedValue.GetValue(i))
                            : Fail(context, ReasonStrings.SubsetDuplicate, field, expectedValue.GetValue(i));
                }
                return Pass();
            }
            return Invalid(context, SUBSET);
        }

        internal static bool Count(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, COUNT, out var expectedValue) && TryField(properties, out var field))
            {
                context.ExpressionTrace(COUNT, field, expectedValue);
                if (!ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object value))
                    return NotHasField(context, field);

                if (value == null)
                    return Fail(context, ReasonStrings.Null, field);

                if (ExpressionHelpers.TryEnumerableLength(value, value: out var actualValue))
                    return Condition(
                        context,
                        actualValue == expectedValue,
                        ReasonStrings.Count,
                        field,
                        actualValue,
                        expectedValue
                    );
            }
            return Invalid(context, COUNT);
        }

        internal static bool Less(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(properties, LESS, out var propertyValue) ||
                !TryOperand(context, LESS, o, properties, out var operand))
                return Invalid(context, LESS);

            if (operand.Value == null)
                return Condition(
                    context,
                    0 < propertyValue,
                    operand,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            if (!ExpressionHelpers.CompareNumeric(
                operand.Value,
                propertyValue,
                convert: false,
                compare: out var compare,
                value: out _))
                return Invalid(context, LESS);

            // int, string, bool
            return Condition(
                context,
                compare < 0,
                operand,
                ReasonStrings.Assert_NotComparedTo,
                operand.Value,
                LESS_THAN,
                propertyValue
            );
        }

        internal static bool LessOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(properties, LESSOREQUALS, out var propertyValue) ||
                !TryOperand(context, LESSOREQUALS, o, properties, out var operand))
                return Invalid(context, LESSOREQUALS);

            if (operand.Value == null)
                return Condition(
                    context,
                    0 <= propertyValue,
                    operand,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            if (!ExpressionHelpers.CompareNumeric(
                operand.Value,
                propertyValue,
                convert: false,
                compare: out var compare,
                value: out _))
                return Invalid(context, LESSOREQUALS);

            // int, string, bool
            return Condition(
                context,
                compare <= 0,
                operand,
                ReasonStrings.Assert_NotComparedTo,
                operand.Value,
                LESS_THAN_EQUALS,
                propertyValue
            );
        }

        internal static bool Greater(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(properties, GREATER, out var propertyValue) ||
                !TryOperand(context, GREATER, o, properties, out var operand))
                return Invalid(context, GREATER);

            if (operand.Value == null)
                return Condition(
                    context,
                    0 > propertyValue,
                    operand,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            if (!ExpressionHelpers.CompareNumeric(
                operand.Value,
                propertyValue,
                convert: false,
                compare: out var compare,
                value: out _))
                return Invalid(context, GREATER);

            // int, string, bool
            return Condition(
                context,
                compare > 0,
                operand,
                ReasonStrings.Assert_NotComparedTo,
                operand.Value,
                GREATER_THAN,
                propertyValue
            );
        }

        internal static bool GreaterOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (!TryPropertyLong(properties, GREATEROREQUALS, out var propertyValue) ||
                !TryOperand(context, GREATEROREQUALS, o, properties, out var operand))
                return Invalid(context, GREATEROREQUALS);

            if (operand.Value == null)
                return Condition(
                    context,
                    0 >= propertyValue,
                    operand,
                    ReasonStrings.Assert_IsNullOrEmpty
                );

            if (!ExpressionHelpers.CompareNumeric(
                operand.Value,
                propertyValue,
                convert: false,
                compare: out var compare,
                value: out _))
                return Invalid(context, GREATEROREQUALS);

            // int, string, bool
            return Condition(
                context,
                compare >= 0,
                operand,
                ReasonStrings.Assert_NotComparedTo,
                operand.Value,
                GREATER_THAN_EQUALS,
                propertyValue
            );
        }

        internal static bool StartsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, STARTSWITH, out var propertyValue) &&
                TryOperand(context, STARTSWITH, o, properties, out var operand))
            {
                context.ExpressionTrace(STARTSWITH, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryString(operand.Value, out var value))
                    return NotString(context, operand);

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                    if (ExpressionHelpers.StartsWith(value, propertyValue[i], caseSensitive: false))
                        return Pass();

                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotStartsWith,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return false;
        }

        internal static bool EndsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, ENDSWITH, out var propertyValue) &&
                TryOperand(context, ENDSWITH, o, properties, out var operand))
            {
                context.ExpressionTrace(ENDSWITH, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryString(operand.Value, out var value))
                    return NotString(context, operand);

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                    if (ExpressionHelpers.EndsWith(value, propertyValue[i], caseSensitive: false))
                        return Pass();

                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotEndsWith,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return false;
        }

        internal static bool Contains(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, CONTAINS, out var propertyValue) &&
                TryOperand(context, CONTAINS, o, properties, out var operand))
            {
                context.ExpressionTrace(CONTAINS, operand.Value, propertyValue);
                if (!ExpressionHelpers.TryString(operand.Value, out var value))
                    return NotString(context, operand);

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                    if (ExpressionHelpers.Contains(value, propertyValue[i], caseSensitive: false))
                        return Pass();

                return Fail(
                    context,
                    operand,
                    ReasonStrings.Assert_NotContains,
                    value,
                    StringJoin(propertyValue)
                );
            }
            return false;
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
                    propertyValue == ExpressionHelpers.TryString(operand.Value, out _),
                    operand,
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
                    propertyValue == ExpressionHelpers.TryArray(operand.Value, out _),
                    operand,
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
                    propertyValue == ExpressionHelpers.TryBool(operand.Value, convert, out _),
                    operand,
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
                    propertyValue == ExpressionHelpers.TryDateTime(operand.Value, convert, out _),
                    operand,
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
                    propertyValue == (ExpressionHelpers.TryInt(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryLong(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryByte(operand.Value, convert, out _)),
                    operand,
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
                    propertyValue == (ExpressionHelpers.TryInt(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryLong(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryFloat(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryByte(operand.Value, convert, out _) ||
                                    ExpressionHelpers.TryDouble(operand.Value, convert, out _)),
                    operand,
                    ReasonStrings.Assert_NotInteger,
                    operand.Value
                );
            }
            return false;
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
                        !propertyValue.Value,
                        operand,
                        ReasonStrings.Assert_NotString,
                        operand.Value
                    );

                context.ExpressionTrace(ISLOWER, operand.Value, propertyValue);
                return Condition(
                    context,
                    propertyValue == ExpressionHelpers.IsLower(value, requireLetters: false, notLetter: out _),
                    operand,
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
                        !propertyValue.Value,
                        operand,
                        ReasonStrings.Assert_NotString,
                        operand.Value
                    );

                context.ExpressionTrace(ISUPPER, operand.Value, propertyValue);
                return Condition(
                    context,
                    propertyValue == ExpressionHelpers.IsUpper(value, requireLetters: false, notLetter: out _),
                    operand,
                    ReasonStrings.Assert_IsUpper,
                    operand.Value
                );
            }
            return false;
        }

        internal static bool HasSchema(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, HASSCHEMA, out var expectedValue) && TryField(properties, out var field) &&
                TryPropertyBoolOrDefault(properties, IGNORESCHEME, out var ignoreScheme, false))
            {
                context.ExpressionTrace(HASSCHEMA, field, expectedValue);
                if (!ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object actualValue))
                    return NotHasField(context, field);

                if (!ObjectHelper.GetPath(context, actualValue, PROPERTY_SCHEMA, caseSensitive: false, out object schemaValue))
                    return NotHasField(context, PROPERTY_SCHEMA);

                if (!ExpressionHelpers.TryString(schemaValue, out var actualSchema))
                    return NotString(context, Operand.FromField(PROPERTY_SCHEMA, schemaValue));

                if (string.IsNullOrEmpty(actualSchema))
                    return NullOrEmpty(context, Operand.FromField(PROPERTY_SCHEMA, schemaValue));

                if (expectedValue == null || expectedValue.Length == 0)
                    return Pass();

                if (ExpressionHelpers.AnySchema(actualSchema, expectedValue, ignoreScheme, false))
                    return Pass();

                return Fail(
                    context,
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

        #endregion Conditions

        #region Helper methods

        private static bool Condition(IExpressionContext context, bool condition, string text, params object[] args)
        {
            if (condition)
                return true;

            context.Reason(text, args);
            return false;
        }

        private static bool Condition(IExpressionContext context, bool condition, IOperand operand, string text, params object[] args)
        {
            if (condition)
                return true;

            context.Reason(operand, text, args);
            return false;
        }

        private static bool Fail(IExpressionContext context, string text, params object[] args)
        {
            return Condition(context, false, text, args);
        }

        private static bool Fail(IExpressionContext context, IOperand operand, string text, params object[] args)
        {
            return Condition(context, false, operand, text, args);
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
        private static bool NotHasField(IExpressionContext context, string field)
        {
            return Fail(context, ReasonStrings.NotHasField, field);
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

        private static bool TryPropertyLong(LanguageExpression.PropertyBag properties, string propertyName, out long? propertyValue)
        {
            return properties.TryGetLong(propertyName, out propertyValue);
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
                operand = Operand.FromField(field, value);

            return operand != null || NotHasField(context, field);
        }

        private static bool TryName(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(NAME, out var svalue))
            {
                if (svalue != ".")
                    return Invalid(context, svalue);

                var binding = context.GetContext()?.TargetBinder?.Using(context.LanguageScope);
                var name = binding.TargetName;
                if (string.IsNullOrEmpty(name))
                    return Invalid(context, svalue);

                operand = Operand.FromName(name);
            }
            return operand != null;
        }

        private static bool TryType(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            operand = null;
            if (properties.TryGetString(TYPE, out var svalue))
            {
                if (svalue != ".")
                    return Invalid(context, svalue);

                var binding = context.GetContext()?.TargetBinder?.Using(context.LanguageScope);
                var type = binding.TargetType;
                if (string.IsNullOrEmpty(type))
                    return Invalid(context, svalue);

                operand = Operand.FromType(type);
            }
            return operand != null;
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
            return properties.TryGetString(FIELD, out var field) && !ObjectHelper.GetPath(context, o, field, caseSensitive: false, out object _);
        }

        private static bool TryOperand(ExpressionContext context, string name, object o, LanguageExpression.PropertyBag properties, out IOperand operand)
        {
            return TryField(context, properties, o, out operand) ||
                TryType(context, properties, out operand) ||
                TryName(context, properties, out operand) ||
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

        #endregion Helper methods
    }
}
