// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Collections.Generic;

namespace PSRule.Definitions.Selectors
{
    internal delegate bool SelectorExpressionFn(SelectorContext context, SelectorInfo info, object[] args, object o);

    internal delegate bool SelectorExpressionOuterFn(SelectorContext context, object o);

    internal enum SelectorExpressionType
    {
        Operator = 1,

        Condition = 2
    }

    internal interface ISelectorExpresssionDescriptor
    {
        string Name { get; }

        SelectorExpressionType Type { get; }

        SelectorExpression CreateInstance(SourceFile source, SelectorExpression.PropertyBag properties);
    }

    internal sealed class SelectorExpresssionDescriptor : ISelectorExpresssionDescriptor
    {
        public SelectorExpresssionDescriptor(string name, SelectorExpressionType type, SelectorExpressionFn fn)
        {
            Name = name;
            Type = type;
            Fn = fn;
        }

        public string Name { get; }

        public SelectorExpressionType Type { get; }

        public SelectorExpressionFn Fn { get; }

        public SelectorExpression CreateInstance(SourceFile source, SelectorExpression.PropertyBag properties)
        {
            if (Type == SelectorExpressionType.Operator)
                return new SelectorOperator(this);

            if (Type == SelectorExpressionType.Condition)
                return new SelectorCondition(this, properties);

            return null;
        }
    }

    internal sealed class SelectorInfo
    {
        private readonly string path;

        public SelectorInfo(string path)
        {
            this.path = path;
        }
    }

    internal sealed class SelectorExpressionFactory
    {
        private readonly Dictionary<string, ISelectorExpresssionDescriptor> _Descriptors;

        public SelectorExpressionFactory()
        {
            _Descriptors = new Dictionary<string, ISelectorExpresssionDescriptor>(SelectorExpressions.Builtin.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var d in SelectorExpressions.Builtin)
                With(d);
        }

        public bool TryDescriptor(string name, out ISelectorExpresssionDescriptor descriptor)
        {
            return _Descriptors.TryGetValue(name, out descriptor);
        }

        public bool IsOperator(string name)
        {
            return TryDescriptor(name, out ISelectorExpresssionDescriptor d) && d != null && d.Type == SelectorExpressionType.Operator;
        }

        public bool IsCondition(string name)
        {
            return TryDescriptor(name, out ISelectorExpresssionDescriptor d) && d != null && d.Type == SelectorExpressionType.Condition;
        }

        private void With(ISelectorExpresssionDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.Name, descriptor);
        }
    }

    internal sealed class SelectorExpressionBuilder
    {
        private const char Dot = '.';
        private const char OpenBracket = '[';
        private const char CloseBracket = '[';

        private readonly bool _Debugger;

        public SelectorExpressionBuilder(bool debugger = true)
        {
            _Debugger = debugger;
        }

        public SelectorExpressionOuterFn Build(SelectorIf selectorIf)
        {
            return Expression(string.Empty, selectorIf.Expression);
        }

        private SelectorExpressionOuterFn Expression(string path, SelectorExpression expression)
        {
            path = Path(path, expression);
            if (expression is SelectorOperator selectorOperator)
                return Debugger(Operator(path, selectorOperator), path);
            else if (expression is SelectorCondition selectorCondition)
                return Debugger(Condition(path, selectorCondition), path);

            throw new InvalidOperationException();
        }

        private static SelectorExpressionOuterFn Condition(string path, SelectorCondition expression)
        {
            var info = new SelectorInfo(path);
            return (context, o) => expression.Descriptor.Fn(context, info, new object[] { expression.Property }, o);
        }

        private static string Path(string path, SelectorExpression expression)
        {
            path = string.Concat(path, Dot, expression.Descriptor.Name);
            return path;
        }

        private SelectorExpressionOuterFn Operator(string path, SelectorOperator expression)
        {
            var inner = new List<SelectorExpressionOuterFn>(expression.Children.Count);
            for (var i = 0; i < expression.Children.Count; i++)
            {
                var childPath = string.Concat(path, OpenBracket, i, CloseBracket);
                inner.Add(Expression(childPath, expression.Children[i]));
            }
            var innerA = inner.ToArray();
            var info = new SelectorInfo(path);
            return (context, o) => expression.Descriptor.Fn(context, info, innerA, o);
        }

        private SelectorExpressionOuterFn Debugger(SelectorExpressionOuterFn expression, string path)
        {
            if (!_Debugger)
                return expression;

            return (context, o) => DebuggerFn(context, path, expression, o);
        }

        private static bool DebuggerFn(SelectorContext context, string path, SelectorExpressionOuterFn expression, object o)
        {
            var result = expression(context, o);
            context.Debug(PSRuleResources.SelectorTrace, path, result);
            return result;
        }
    }

    /// <summary>
    /// Expressions that can be used with selectors.
    /// </summary>
    internal sealed class SelectorExpressions
    {
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

        private const string IF = "if";
        private const string ANYOF = "anyOf";
        private const string ALLOF = "allOf";
        private const string NOT = "not";
        private const string FIELD = "field";

        // Define built-ins
        internal readonly static ISelectorExpresssionDescriptor[] Builtin = new ISelectorExpresssionDescriptor[]
        {
            // Operators
            new SelectorExpresssionDescriptor(IF, SelectorExpressionType.Operator, If),
            new SelectorExpresssionDescriptor(ANYOF, SelectorExpressionType.Operator, AnyOf),
            new SelectorExpresssionDescriptor(ALLOF, SelectorExpressionType.Operator, AllOf),
            new SelectorExpresssionDescriptor(NOT, SelectorExpressionType.Operator, Not),

            // Conditions
            new SelectorExpresssionDescriptor(EXISTS, SelectorExpressionType.Condition, Exists),
            new SelectorExpresssionDescriptor(EQUALS, SelectorExpressionType.Condition, Equals),
            new SelectorExpresssionDescriptor(NOTEQUALS, SelectorExpressionType.Condition, NotEquals),
            new SelectorExpresssionDescriptor(HASVALUE, SelectorExpressionType.Condition, HasValue),
            new SelectorExpresssionDescriptor(MATCH, SelectorExpressionType.Condition, Match),
            new SelectorExpresssionDescriptor(NOTMATCH, SelectorExpressionType.Condition, NotMatch),
            new SelectorExpresssionDescriptor(IN, SelectorExpressionType.Condition, In),
            new SelectorExpresssionDescriptor(NOTIN, SelectorExpressionType.Condition, NotIn),
            new SelectorExpresssionDescriptor(LESS, SelectorExpressionType.Condition, Less),
            new SelectorExpresssionDescriptor(LESSOREQUALS, SelectorExpressionType.Condition, LessOrEquals),
            new SelectorExpresssionDescriptor(GREATER, SelectorExpressionType.Condition, Greater),
            new SelectorExpresssionDescriptor(GREATEROREQUALS, SelectorExpressionType.Condition, GreaterOrEquals),
        };

        #region Operators

        internal static bool If(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            if (inner.Length > 0)
                return inner[0](context, o);

            return false;
        }

        internal static bool AnyOf(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (inner[i](context, o))
                    return true;
            }
            return false;
        }

        internal static bool AllOf(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (!inner[i](context, o))
                    return false;
            }
            return true;
        }

        internal static bool Not(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            if (inner.Length > 0)
                return !inner[0](context, o);

            return false;
        }

        #endregion Operators

        #region Conditions

        internal static bool Exists(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, EXISTS, out bool? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, EXISTS, field, propertyValue);
                return propertyValue == ExpressionHelpers.Exists(context, o, field, caseSensitive: false);
            }
            return false;
        }

        internal static bool Equals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, EQUALS, out object propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, EQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return false;

                // int, string, bool
                return ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true);
            }
            return false;
        }

        internal static bool NotEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTEQUALS, out object propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, NOTEQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                // int, string, bool
                return !ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true);
            }
            return false;
        }

        internal static bool HasValue(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, HASVALUE, out bool? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, HASVALUE, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return !propertyValue.Value;

                return !propertyValue.Value == ExpressionHelpers.NullOrEmpty(value);
            }
            return false;
        }

        internal static bool Match(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, MATCH, out object propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, MATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                return ExpressionHelpers.Match(propertyValue, value, caseSensitive: false);
            }
            return false;
        }

        internal static bool NotMatch(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTMATCH, out object propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, NOTMATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                return !ExpressionHelpers.Match(propertyValue, value, caseSensitive: false);
            }
            return false;
        }

        internal static bool In(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, IN, out Array propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, IN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return false;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return true;
                }
                return false;
            }
            return false;
        }

        internal static bool NotIn(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, NOTIN, out Array propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, NOTIN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return false;
                }
                return true;
            }
            return false;
        }

        internal static bool Less(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, LESS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                if (value == null)
                    return 0 < propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return compare < 0;
            }
            return false;
        }

        internal static bool LessOrEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESSOREQUALS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, LESSOREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                if (value == null)
                    return 0 <= propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return compare <= 0;
            }
            return false;
        }

        internal static bool Greater(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATER, out long? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, GREATER, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                if (value == null)
                    return 0 > propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return compare > 0;
            }
            return false;
        }

        internal static bool GreaterOrEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATEROREQUALS, out long? propertyValue) && TryField(properties, out string field))
            {
                context.Debug(PSRuleResources.SelectorExpressionTrace, GREATEROREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out object value))
                    return true;

                if (value == null)
                    return 0 >= propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out int compare, value: out _))
                    return compare >= 0;
            }
            return false;
        }

        #endregion Conditions

        #region Helper methods

        private static bool TryProperty(SelectorExpression.PropertyBag properties, string propertyName, out object propertyValue)
        {
            return properties.TryGetValue(propertyName, out propertyValue);
        }

        private static bool TryPropertyBool(SelectorExpression.PropertyBag properties, string propertyName, out bool? propertyValue)
        {
            return properties.TryGetBool(propertyName, out propertyValue);
        }

        private static bool TryPropertyLong(SelectorExpression.PropertyBag properties, string propertyName, out long? propertyValue)
        {
            return properties.TryGetLong(propertyName, out propertyValue);
        }

        private static bool TryField(SelectorExpression.PropertyBag properties, out string field)
        {
            return properties.TryGetString(FIELD, out field);
        }

        private static bool TryPropertyArray(SelectorExpression.PropertyBag properties, string propertyName, out Array propertyValue)
        {
            if (properties.TryGetValue(propertyName, out object array) && array is Array arrayValue)
            {
                propertyValue = arrayValue;
                return true;
            }
            propertyValue = null;
            return false;
        }

        private static SelectorExpression.PropertyBag GetProperties(object[] args)
        {
            return (SelectorExpression.PropertyBag)args[0];
        }

        private static SelectorExpressionOuterFn[] GetInner(object[] args)
        {
            return (SelectorExpressionOuterFn[])args;
        }

        #endregion Helper methods
    }
}
