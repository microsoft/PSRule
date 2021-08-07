// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Collections.Generic;

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
            return TryDescriptor(name, out ILanguageExpresssionDescriptor d) && d != null && d.Type == LanguageExpressionType.Operator;
        }

        public bool IsCondition(string name)
        {
            return TryDescriptor(name, out ILanguageExpresssionDescriptor d) && d != null && d.Type == LanguageExpressionType.Condition;
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

        public LanguageExpressionOuterFn Build(LanguageIf selectorIf)
        {
            return Precondition(Expression(string.Empty, selectorIf.Expression), _With, _Type);
        }

        private static LanguageExpressionOuterFn Precondition(LanguageExpressionOuterFn expression, string[] with, string[] type)
        {
            var fn = expression;
            if (type != null)
                fn = PreconditionType(type, fn);

            if (with != null)
                fn = PreconditionSelector(with, fn);

            return fn;
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
            if (!_Debugger)
                return expression;

            return (context, o) => DebuggerFn(context, path, expression, o);
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

            var comparer = RunspaceContext.CurrentThread.Pipeline.Baseline.GetTargetBinding().IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
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
        private const string ISLOWER = "isLower";
        private const string ISUPPER = "isUpper";

        // Operators
        private const string IF = "if";
        private const string ANYOF = "anyOf";
        private const string ALLOF = "allOf";
        private const string NOT = "not";

        // Properties
        private const string FIELD = "field";

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
            new LanguageExpresssionDescriptor(ISLOWER, LanguageExpressionType.Condition, IsLower),
            new LanguageExpresssionDescriptor(ISUPPER, LanguageExpressionType.Condition, IsUpper),
        };

        #region Operators

        internal static bool If(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            if (inner.Length > 0)
                return inner[0](context, o) ?? true;

            return false;
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
            if (inner.Length > 0)
                return !inner[0](context, o) ?? false;

            return false;
        }

        #endregion Operators

        #region Conditions

        internal static bool Exists(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, EXISTS, out bool? propertyValue) && TryField(properties, out string field))
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
            if (TryProperty(properties, EQUALS, out object propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(EQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return NotHasField(context, field);

                // int, string, bool
                return Condition(
                    context,
                    ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true),
                    ReasonStrings.HasExpectedFieldValue,
                    field,
                    value
                );
            }
            return Invalid(context, EQUALS);
        }

        internal static bool NotEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTEQUALS, out object propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(NOTEQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                // int, string, bool
                return Condition(
                    context,
                    !ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true),
                    ReasonStrings.HasExpectedFieldValue,
                    field,
                    value
                );
            }
            return Invalid(context, NOTEQUALS);
        }

        internal static bool HasValue(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, HASVALUE, out bool? propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(HASVALUE, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Condition(
                        context,
                        !propertyValue.Value,
                        ReasonStrings.NotHasField,
                        field
                    );

                return Condition(
                    context,
                    !propertyValue.Value == ExpressionHelpers.NullOrEmpty(value),
                    ReasonStrings.NotHasFieldValue,
                    field
                );
            }
            return Invalid(context, HASVALUE);
        }

        internal static bool Match(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, MATCH, out object propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(MATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return NotHasField(context, field);

                return Condition(
                    context,
                    ExpressionHelpers.Match(propertyValue, value, caseSensitive: false),
                    ReasonStrings.Match,
                    propertyValue
                );
            }
            return Invalid(context, MATCH);
        }

        internal static bool NotMatch(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTMATCH, out object propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(NOTMATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                return Condition(
                    context,
                    !ExpressionHelpers.Match(propertyValue, value, caseSensitive: false),
                    ReasonStrings.MatchNot,
                    propertyValue
                );
            }
            return Invalid(context, NOTMATCH);
        }

        internal static bool In(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, IN, out Array propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(IN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return NotHasField(context, field);

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return Pass();
                }
                return Fail(context, ReasonStrings.In, value);
            }
            return Invalid(context, IN);
        }

        internal static bool NotIn(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, NOTIN, out Array propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(NOTIN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return Fail(context, ReasonStrings.NotIn, value);
                }
                return Pass();
            }
            return Invalid(context, NOTIN);
        }

        internal static bool Less(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(LESS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                if (value == null)
                    return Condition(
                        context,
                        0 < propertyValue,
                        ReasonStrings.Null,
                        field
                    );

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return Condition(
                        context,
                        compare < 0,
                        ReasonStrings.Less,
                        value,
                        propertyValue
                    );
            }
            return Invalid(context, LESS);
        }

        internal static bool LessOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESSOREQUALS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(LESSOREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                if (value == null)
                    return Condition(
                        context,
                        0 <= propertyValue,
                        ReasonStrings.Null,
                        field
                    );

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return Condition(
                        context,
                        compare <= 0,
                        ReasonStrings.LessOrEqual,
                        value,
                        propertyValue
                    );
            }
            return Invalid(context, LESSOREQUALS);
        }

        internal static bool Greater(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATER, out long? propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(GREATER, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                if (value == null)
                    return Condition(
                        context,
                        0 > propertyValue,
                        ReasonStrings.Null,
                        field
                    );

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return Condition(
                        context,
                        compare > 0,
                        ReasonStrings.Greater,
                        value,
                        propertyValue
                    );
            }
            return Invalid(context, GREATER);
        }

        internal static bool GreaterOrEquals(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATEROREQUALS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.ExpressionTrace(GREATEROREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return Pass();

                if (value == null)
                    return Condition(
                        context,
                        0 >= propertyValue,
                        ReasonStrings.Null,
                        field
                    );

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return Condition(
                        context,
                        compare >= 0,
                        ReasonStrings.GreaterOrEqual,
                        value,
                        propertyValue
                    );
            }
            return Invalid(context, GREATEROREQUALS);
        }

        internal static bool StartsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, STARTSWITH, out string[] propertyValue) && TryOperand(context, STARTSWITH, o, properties, out object operand))
            {
                context.ExpressionTrace(STARTSWITH, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out string value))
                    return Fail(
                        context,
                        ReasonStrings.String,
                        operand
                    );

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.StartsWith(value, propertyValue[i], caseSensitive: false))
                        return Pass();
                }
                return Fail(
                    context,
                    ReasonStrings.StartsWith,
                    value,
                    propertyValue
                );
            }
            return false;
        }

        internal static bool EndsWith(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, ENDSWITH, out string[] propertyValue) && TryOperand(context, ENDSWITH, o, properties, out object operand))
            {
                context.ExpressionTrace(ENDSWITH, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out string value))
                    return Fail(
                        context,
                        ReasonStrings.String,
                        operand
                    );

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.EndsWith(value, propertyValue[i], caseSensitive: false))
                        return Pass();
                }
                return Fail(
                    context,
                    ReasonStrings.EndsWith,
                    value,
                    propertyValue
                );
            }
            return false;
        }

        internal static bool Contains(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, CONTAINS, out string[] propertyValue) && TryOperand(context, CONTAINS, o, properties, out object operand))
            {
                context.ExpressionTrace(CONTAINS, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out string value))
                    return Fail(
                        context,
                        ReasonStrings.String,
                        operand
                    );

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.Contains(value, propertyValue[i], caseSensitive: false))
                        return Pass();
                }
                return Fail(
                    context,
                    ReasonStrings.Contains,
                    value,
                    propertyValue
                );
            }
            return false;
        }

        internal static bool IsString(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISSTRING, out bool? propertyValue) && TryOperand(context, ISSTRING, o, properties, out object operand))
            {
                context.ExpressionTrace(ISSTRING, operand, propertyValue);
                return Condition(
                    context,
                    propertyValue == ExpressionHelpers.TryString(operand, out _),
                    ReasonStrings.String,
                    operand
                );
            }
            return false;
        }

        internal static bool IsLower(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISLOWER, out bool? propertyValue) && TryOperand(context, ISLOWER, o, properties, out object operand))
            {
                if (!ExpressionHelpers.TryString(operand, out string value))
                    return Condition(
                        context,
                        !propertyValue.Value,
                        ReasonStrings.String,
                        operand
                    );

                context.ExpressionTrace(ISLOWER, operand, propertyValue);
                return Condition(
                    context,
                    propertyValue == ExpressionHelpers.IsLower(value, requireLetters: false, notLetter: out _),
                    ReasonStrings.IsLower,
                    operand
                );
            }
            return false;
        }

        internal static bool IsUpper(ExpressionContext context, ExpressionInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISUPPER, out bool? propertyValue) && TryOperand(context, ISUPPER, o, properties, out object operand))
            {
                if (!ExpressionHelpers.TryString(operand, out string value))
                    return Condition(
                        context,
                        !propertyValue.Value,
                        ReasonStrings.String,
                        operand
                    );

                context.ExpressionTrace(ISUPPER, operand, propertyValue);
                return Condition(
                    context,
                    propertyValue == ExpressionHelpers.IsUpper(value, requireLetters: false, notLetter: out _),
                    ReasonStrings.IsUpper,
                    operand
                );
            }
            return false;
        }

        #endregion Conditions

        #region Helper methods

        private static bool Condition(ExpressionContext context, bool condition, string text, params object[] args)
        {
            if (condition)
                return true;

            context.Reason(text, args);
            return false;
        }

        private static bool Fail(ExpressionContext context, string text, params object[] args)
        {
            return Condition(context, false, text, args);
        }

        private static bool Pass()
        {
            return true;
        }

        private static bool Invalid(ExpressionContext context, string name)
        {
            return false;
        }

        private static bool NotHasField(ExpressionContext context, string field)
        {
            return Fail(context, ReasonStrings.NotHasField, field);
        }

        private static bool TryProperty(LanguageExpression.PropertyBag properties, string propertyName, out object propertyValue)
        {
            return properties.TryGetValue(propertyName, out propertyValue);
        }

        private static bool TryPropertyBool(LanguageExpression.PropertyBag properties, string propertyName, out bool? propertyValue)
        {
            return properties.TryGetBool(propertyName, out propertyValue);
        }

        private static bool TryPropertyLong(LanguageExpression.PropertyBag properties, string propertyName, out long? propertyValue)
        {
            return properties.TryGetLong(propertyName, out propertyValue);
        }

        private static bool TryField(LanguageExpression.PropertyBag properties, out string field)
        {
            return properties.TryGetString(FIELD, out field);
        }

        private static bool TryOperand(ExpressionContext context, string name, object o, LanguageExpression.PropertyBag properties, out object operand)
        {
            operand = null;
            if (properties.TryGetString(FIELD, out string field))
                return ObjectHelper.GetField(context, o, field, caseSensitive: false, out operand);

            return Invalid(context, name);
        }

        private static bool TryPropertyArray(LanguageExpression.PropertyBag properties, string propertyName, out Array propertyValue)
        {
            if (properties.TryGetValue(propertyName, out object array) && array is Array arrayValue)
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
            else if (properties.TryGetString(propertyName, out string s))
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

        #endregion Helper methods
    }
}
