// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

internal delegate bool LanguageExpressionFn(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o);

internal delegate bool? LanguageExpressionOuterFn(IExpressionContext context, ITargetObject o);

/// <summary>
/// Expressions that can be used with selectors.
/// </summary>
internal sealed class LanguageExpressions
{
    // Conditions
    private const string EXISTS = "exists";
    private const string EQUALS = "equals";
    private const string NOT_EQUALS = "notEquals";
    private const string HAS_DEFAULT = "hasDefault";
    private const string HAS_SCHEMA = "hasSchema";
    private const string HAS_VALUE = "hasValue";
    private const string MATCH = "match";
    private const string NOT_MATCH = "notMatch";
    private const string IN = "in";
    private const string NOT_IN = "notIn";
    private const string LESS = "less";
    private const string LESS_OR_EQUALS = "lessOrEquals";
    private const string GREATER = "greater";
    private const string GREATER_OR_EQUALS = "greaterOrEquals";
    private const string STARTS_WITH = "startsWith";
    private const string NOT_STARTS_WITH = "notStartsWith";
    private const string ENDS_WITH = "endsWith";
    private const string NOT_ENDS_WITH = "notEndsWith";
    private const string CONTAINS = "contains";
    private const string NOT_CONTAINS = "notContains";
    private const string IS_STRING = "isString";
    private const string IS_ARRAY = "isArray";
    private const string IS_BOOLEAN = "isBoolean";
    private const string IS_DATETIME = "isDateTime";
    private const string IS_INTEGER = "isInteger";
    private const string IS_NUMERIC = "IsNumeric";
    private const string IS_LOWER = "isLower";
    private const string IS_UPPER = "isUpper";
    private const string SET_OF = "setOf";
    private const string SUBSET = "subset";
    private const string COUNT = "count";
    private const string NOTCOUNT = "notCount";
    private const string VERSION = "version";
    private const string API_VERSION = "apiVersion";
    private const string WITHIN_PATH = "withinPath";
    private const string NOT_WITHIN_PATH = "notWithinPath";
    private const string LIKE = "like";
    private const string NOT_LIKE = "notLike";

    // Operators
    private const string IF = "if";
    private const string ANYOF = "anyOf";
    private const string ALLOF = "allOf";
    private const string NOT = "not";

    // Properties
    private const string FIELD = "field";
    private const string TYPE = "type";
    private const string NAME = "name";
    private const string CASE_SENSITIVE = "caseSensitive";
    private const string UNIQUE = "unique";
    private const string CONVERT = "convert";
    private const string IGNORE_SCHEME = "ignoreScheme";
    private const string INCLUDE_PRERELEASE = "includePrerelease";
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
    internal static readonly ILanguageExpressionDescriptor[] Builtin =
    [
        // Operators
        new LanguageExpressionDescriptor(IF, LanguageExpressionType.Operator, If),
        new LanguageExpressionDescriptor(ANYOF, LanguageExpressionType.Operator, AnyOf),
        new LanguageExpressionDescriptor(ALLOF, LanguageExpressionType.Operator, AllOf),
        new LanguageExpressionDescriptor(NOT, LanguageExpressionType.Operator, Not),

        // Conditions
        new LanguageExpressionDescriptor(EXISTS, LanguageExpressionType.Condition, Exists),
        new LanguageExpressionDescriptor(EQUALS, LanguageExpressionType.Condition, Equals),
        new LanguageExpressionDescriptor(NOT_EQUALS, LanguageExpressionType.Condition, NotEquals),
        new LanguageExpressionDescriptor(HAS_VALUE, LanguageExpressionType.Condition, HasValue),
        new LanguageExpressionDescriptor(MATCH, LanguageExpressionType.Condition, Match),
        new LanguageExpressionDescriptor(NOT_MATCH, LanguageExpressionType.Condition, NotMatch),
        new LanguageExpressionDescriptor(IN, LanguageExpressionType.Condition, In),
        new LanguageExpressionDescriptor(NOT_IN, LanguageExpressionType.Condition, NotIn),
        new LanguageExpressionDescriptor(LESS, LanguageExpressionType.Condition, Less),
        new LanguageExpressionDescriptor(LESS_OR_EQUALS, LanguageExpressionType.Condition, LessOrEquals),
        new LanguageExpressionDescriptor(GREATER, LanguageExpressionType.Condition, Greater),
        new LanguageExpressionDescriptor(GREATER_OR_EQUALS, LanguageExpressionType.Condition, GreaterOrEquals),
        new LanguageExpressionDescriptor(STARTS_WITH, LanguageExpressionType.Condition, StartsWith),
        new LanguageExpressionDescriptor(NOT_STARTS_WITH, LanguageExpressionType.Condition, NotStartsWith),
        new LanguageExpressionDescriptor(ENDS_WITH, LanguageExpressionType.Condition, EndsWith),
        new LanguageExpressionDescriptor(NOT_ENDS_WITH, LanguageExpressionType.Condition, NotEndsWith),
        new LanguageExpressionDescriptor(CONTAINS, LanguageExpressionType.Condition, Contains),
        new LanguageExpressionDescriptor(NOT_CONTAINS, LanguageExpressionType.Condition, NotContains),
        new LanguageExpressionDescriptor(IS_STRING, LanguageExpressionType.Condition, IsString),
        new LanguageExpressionDescriptor(IS_ARRAY, LanguageExpressionType.Condition, IsArray),
        new LanguageExpressionDescriptor(IS_BOOLEAN, LanguageExpressionType.Condition, IsBoolean),
        new LanguageExpressionDescriptor(IS_DATETIME, LanguageExpressionType.Condition, IsDateTime),
        new LanguageExpressionDescriptor(IS_INTEGER, LanguageExpressionType.Condition, IsInteger),
        new LanguageExpressionDescriptor(IS_NUMERIC, LanguageExpressionType.Condition, IsNumeric),
        new LanguageExpressionDescriptor(IS_LOWER, LanguageExpressionType.Condition, IsLower),
        new LanguageExpressionDescriptor(IS_UPPER, LanguageExpressionType.Condition, IsUpper),
        new LanguageExpressionDescriptor(SET_OF, LanguageExpressionType.Condition, SetOf),
        new LanguageExpressionDescriptor(SUBSET, LanguageExpressionType.Condition, Subset),
        new LanguageExpressionDescriptor(COUNT, LanguageExpressionType.Condition, Count),
        new LanguageExpressionDescriptor(NOTCOUNT, LanguageExpressionType.Condition, NotCount),
        new LanguageExpressionDescriptor(HAS_SCHEMA, LanguageExpressionType.Condition, HasSchema),
        new LanguageExpressionDescriptor(VERSION, LanguageExpressionType.Condition, Version),
        new LanguageExpressionDescriptor(API_VERSION, LanguageExpressionType.Condition, APIVersion),
        new LanguageExpressionDescriptor(HAS_DEFAULT, LanguageExpressionType.Condition, HasDefault),
        new LanguageExpressionDescriptor(WITHIN_PATH, LanguageExpressionType.Condition, WithinPath),
        new LanguageExpressionDescriptor(NOT_WITHIN_PATH, LanguageExpressionType.Condition, NotWithinPath),
        new LanguageExpressionDescriptor(LIKE, LanguageExpressionType.Condition, Like),
        new LanguageExpressionDescriptor(NOT_LIKE, LanguageExpressionType.Condition, NotLike),
    ];

    #region Operators

    internal static bool If(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var inner = GetInner(args);
        return inner.Length > 0 && (inner[0](context, o) ?? true);
    }

    internal static bool AnyOf(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var inner = GetInner(args);
        for (var i = 0; i < inner.Length; i++)
        {
            if (inner[i](context, o) ?? false)
                return true;
        }
        return false;
    }

    internal static bool AllOf(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var inner = GetInner(args);
        for (var i = 0; i < inner.Length; i++)
        {
            if (!inner[i](context, o) ?? true)
                return false;
        }
        return true;
    }

    internal static bool Not(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var inner = GetInner(args);
        return inner.Length > 0 && (!inner[0](context, o) ?? false);
    }

    #endregion Operators

    #region Conditions

    internal static bool Exists(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, EXISTS, out var propertyValue) && propertyValue != null && TryField(properties, out var field) && field != null)
        {
            context.ExpressionTrace(EXISTS, field, propertyValue);
            return Condition(
                context,
                field,
                propertyValue == ExpressionHelpers.Exists(context, o.Value, field, caseSensitive: false),
                ReasonStrings.Exists,
                field
            );
        }
        return Invalid(context, EXISTS);
    }

    internal static bool Equals(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool NotEquals(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyAny(properties, NOT_EQUALS, out var propertyValue))
            return Invalid(context, NOT_EQUALS);

        if (TryFieldNotExists(context, o, properties))
            return PassPathNotFound(context, NOT_EQUALS);

        if (!TryOperand(context, NOT_EQUALS, o, properties, out var operand) ||
            !GetConvert(properties, out var convert) ||
            !GetCaseSensitive(properties, out var caseSensitive))
            return Invalid(context, NOT_EQUALS);

        // int, string, bool
        return Condition(
            context,
            operand,
            !ExpressionHelpers.Equal(Value(context, propertyValue), Value(context, operand), caseSensitive, convertExpected: true, convertActual: convert),
            ReasonStrings.Assert_IsSetTo,
            operand.Value
        );
    }

    internal static bool HasDefault(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyAny(properties, HAS_DEFAULT, out var propertyValue))
            return Invalid(context, HAS_DEFAULT);

        GetCaseSensitive(properties, out var caseSensitive);
        if (TryFieldNotExists(context, o, properties))
            return PassPathNotFound(context, HAS_DEFAULT);

        if (!TryOperand(context, HAS_DEFAULT, o, properties, out var operand) || operand == null)
            return Invalid(context, HAS_DEFAULT);

        return Condition(
            context,
            operand,
            ExpressionHelpers.Equal(propertyValue, operand.Value, caseSensitive, convertExpected: true),
            ReasonStrings.Assert_IsSetTo,
            operand.Value
        );
    }

    internal static bool HasValue(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyBool(properties, HAS_VALUE, out var propertyValue))
            return Invalid(context, HAS_VALUE);

        if (TryFieldNotExists(context, o, properties) && !propertyValue.Value)
            return PassPathNotFound(context, HAS_VALUE);

        if (!TryOperand(context, HAS_VALUE, o, properties, out var operand) || operand == null)
            return Invalid(context, HAS_VALUE);

        return Condition(
            context,
            operand,
            !propertyValue.Value == ExpressionHelpers.NullOrEmpty(operand.Value),
            ReasonStrings.Assert_IsSetTo,
            operand.Value
        );
    }

    internal static bool Match(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool NotMatch(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyAny(properties, NOT_MATCH, out var propertyValue))
            return Invalid(context, NOT_MATCH);

        if (TryFieldNotExists(context, o, properties))
            return PassPathNotFound(context, NOT_MATCH);

        if (!TryOperand(context, NOT_MATCH, o, properties, out var operand) ||
            !GetCaseSensitive(properties, out var caseSensitive))
            return Invalid(context, NOT_MATCH);

        return Condition(
            context,
            operand,
            !ExpressionHelpers.Match(propertyValue, operand.Value, caseSensitive),
            ReasonStrings.Assert_Matches,
            operand.Value,
            propertyValue
        );
    }

    internal static bool In(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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
            StringJoinArray(propertyValue)
        );
    }

    internal static bool NotIn(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyArray(properties, NOT_IN, out var propertyValue))
            return Invalid(context, NOT_IN);

        if (TryFieldNotExists(context, o, properties))
            return PassPathNotFound(context, NOT_IN);

        if (!TryOperand(context, NOT_IN, o, properties, out var operand))
            return Invalid(context, NOT_IN);

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

    internal static bool SetOf(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyArray(properties, SET_OF, out var expectedValue) &&
            TryField(properties, out var field) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(SET_OF, field, expectedValue);
            if (!ObjectHelper.GetPath(context, o.Value, field, caseSensitive: false, out object actualValue))
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
        return Invalid(context, SET_OF);
    }

    internal static bool Subset(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyArray(properties, SUBSET, out var expectedValue) && TryField(properties, out var field) &&
            GetCaseSensitive(properties, out var caseSensitive) && GetUnique(properties, out var unique))
        {
            context.ExpressionTrace(SUBSET, field, expectedValue);
            if (!ObjectHelper.GetPath(context, o.Value, field, caseSensitive: false, out object actualValue))
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

    internal static bool Count(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool NotCount(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool Less(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool LessOrEquals(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyLong(context, properties, LESS_OR_EQUALS, out var propertyValue) ||
            !TryOperand(context, LESS_OR_EQUALS, o, properties, out var operand) ||
            !GetConvert(properties, out var convert))
            return Invalid(context, LESS_OR_EQUALS);

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
            return Invalid(context, LESS_OR_EQUALS);

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

    internal static bool Greater(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool GreaterOrEquals(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (!TryPropertyLong(context, properties, GREATER_OR_EQUALS, out var propertyValue) ||
            !TryOperand(context, GREATER_OR_EQUALS, o, properties, out var operand) ||
            !GetConvert(properties, out var convert))
            return Invalid(context, GREATER_OR_EQUALS);

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
            return Invalid(context, GREATER_OR_EQUALS);

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

    internal static bool StartsWith(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, STARTS_WITH, out var propertyValue) &&
            TryOperand(context, STARTS_WITH, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(STARTS_WITH, operand.Value, propertyValue);
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
                StringJoin(value),
                StringJoin(propertyValue)
            );
        }
        return Invalid(context, STARTS_WITH);
    }

    internal static bool NotStartsWith(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, NOT_STARTS_WITH, out var propertyValue) &&
            TryOperand(context, NOT_STARTS_WITH, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(NOT_STARTS_WITH, operand.Value, propertyValue);
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
        return Invalid(context, NOT_STARTS_WITH);
    }

    internal static bool EndsWith(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, ENDS_WITH, out var propertyValue) &&
            TryOperand(context, ENDS_WITH, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(ENDS_WITH, operand.Value, propertyValue);
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
        return Invalid(context, ENDS_WITH);
    }

    internal static bool NotEndsWith(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, NOT_ENDS_WITH, out var propertyValue) &&
            TryOperand(context, NOT_ENDS_WITH, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(NOT_ENDS_WITH, operand.Value, propertyValue);
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
        return Invalid(context, NOT_ENDS_WITH);
    }

    internal static bool Contains(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool NotContains(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, NOT_CONTAINS, out var propertyValue) &&
            TryOperand(context, NOT_CONTAINS, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(NOT_CONTAINS, operand.Value, propertyValue);
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
        return Invalid(context, NOT_CONTAINS);
    }

    internal static bool IsString(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_STRING, out var propertyValue) &&
            TryOperand(context, IS_STRING, o, properties, out var operand))
        {
            context.ExpressionTrace(IS_STRING, operand.Value, propertyValue);
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

    internal static bool IsArray(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_ARRAY, out var propertyValue) &&
            TryOperand(context, IS_ARRAY, o, properties, out var operand))
        {
            context.ExpressionTrace(IS_ARRAY, operand.Value, propertyValue);
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

    internal static bool IsBoolean(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_BOOLEAN, out var propertyValue) &&
            TryOperand(context, IS_BOOLEAN, o, properties, out var operand) &&
            GetConvert(properties, out var convert))
        {
            context.ExpressionTrace(IS_BOOLEAN, operand.Value, propertyValue);
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

    internal static bool IsDateTime(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_DATETIME, out var propertyValue) &&
            TryOperand(context, IS_DATETIME, o, properties, out var operand) &&
            GetConvert(properties, out var convert))
        {
            context.ExpressionTrace(IS_DATETIME, operand.Value, propertyValue);
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

    internal static bool IsInteger(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_INTEGER, out var propertyValue) &&
            TryOperand(context, IS_INTEGER, o, properties, out var operand) &&
            GetConvert(properties, out var convert))
        {
            context.ExpressionTrace(IS_INTEGER, operand.Value, propertyValue);
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

    internal static bool IsNumeric(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_NUMERIC, out var propertyValue) &&
            TryOperand(context, IS_NUMERIC, o, properties, out var operand) &&
            GetConvert(properties, out var convert))
        {
            context.ExpressionTrace(IS_NUMERIC, operand.Value, propertyValue);
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

    internal static bool WithinPath(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryOperand(context, WITHIN_PATH, o, properties, out var operand) &&
            TryPropertyStringArray(properties, WITHIN_PATH, out var path) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(WITHIN_PATH, operand.Value, path);

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

        return Invalid(context, WITHIN_PATH);
    }

    internal static bool NotWithinPath(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryOperand(context, NOT_WITHIN_PATH, o, properties, out var operand) &&
            TryPropertyStringArray(properties, NOT_WITHIN_PATH, out var path) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(NOT_WITHIN_PATH, operand.Value, path);

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

        return Invalid(context, NOT_WITHIN_PATH);
    }

    internal static bool Like(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
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

    internal static bool NotLike(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyStringArray(properties, NOT_LIKE, out var propertyValue) &&
            TryOperand(context, NOT_LIKE, o, properties, out var operand) &&
            GetConvert(properties, out var convert) &&
            GetCaseSensitive(properties, out var caseSensitive))
        {
            context.ExpressionTrace(NOT_LIKE, operand.Value, propertyValue);
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
        return Invalid(context, NOT_LIKE);
    }

    internal static bool IsLower(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_LOWER, out var propertyValue) &&
            TryOperand(context, IS_LOWER, o, properties, out var operand))
        {
            if (!ExpressionHelpers.TryString(operand.Value, out var value) || value == null)
                return Condition(
                    context,
                    operand,
                    !propertyValue.Value,
                    ReasonStrings.Assert_NotString,
                    operand.Value
                );

            context.ExpressionTrace(IS_LOWER, operand.Value, propertyValue);
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

    internal static bool IsUpper(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyBool(properties, IS_UPPER, out var propertyValue) &&
            TryOperand(context, IS_UPPER, o, properties, out var operand))
        {
            if (!ExpressionHelpers.TryString(operand.Value, out var value) || value == null)
                return Condition(
                    context,
                    operand,
                    !propertyValue.Value,
                    ReasonStrings.Assert_NotString,
                    operand.Value
                );

            context.ExpressionTrace(IS_UPPER, operand.Value, propertyValue);
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

    internal static bool HasSchema(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyArray(properties, HAS_SCHEMA, out var expectedValue) &&
            TryField(properties, out var field) &&
            TryPropertyBoolOrDefault(properties, IGNORE_SCHEME, out var ignoreScheme, false))
        {
            context.ExpressionTrace(HAS_SCHEMA, field, expectedValue);
            if (!ObjectHelper.GetPath(context, o.Value, field, caseSensitive: false, out object actualValue))
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
        return Invalid(context, HAS_SCHEMA);
    }

    internal static bool Version(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyString(properties, VERSION, out var expectedValue) &&
            TryOperand(context, VERSION, o, properties, out var operand) &&
            TryPropertyBoolOrDefault(properties, INCLUDE_PRERELEASE, out var includePrerelease, false))
        {
            context.ExpressionTrace(VERSION, operand.Value, expectedValue);
            if (!ExpressionHelpers.TryString(operand.Value, out var version))
                return NotString(context, operand);

            if (!SemanticVersion.TryParseVersion(version, out var actualVersion))
                return Fail(context, operand, ReasonStrings.Version, operand.Value);

            if (!SemanticVersion.TryParseConstraint(expectedValue, out var constraint, includePrerelease))
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, expectedValue));

            if (constraint != null && !constraint.Accepts(actualVersion))
                return Fail(context, operand, ReasonStrings.VersionConstraint, actualVersion, constraint);

            return Pass();
        }
        return Invalid(context, VERSION);
    }

    internal static bool APIVersion(IExpressionContext context, ExpressionInfo info, object[] args, ITargetObject o)
    {
        var properties = GetProperties(args);
        if (TryPropertyString(properties, API_VERSION, out var expectedValue) &&
            TryOperand(context, API_VERSION, o, properties, out var operand) &&
            TryPropertyBoolOrDefault(properties, INCLUDE_PRERELEASE, out var includePrerelease, false))
        {
            context.ExpressionTrace(API_VERSION, operand.Value, expectedValue);
            if (!ExpressionHelpers.TryString(operand.Value, out var version))
                return NotString(context, operand);

            if (!DateVersion.TryParseVersion(version, out var actualVersion))
                return Fail(context, operand, ReasonStrings.Version, operand.Value);

            if (!DateVersion.TryParseConstraint(expectedValue, out var constraint, includePrerelease))
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, expectedValue));

            if (constraint != null && !constraint.Accepts(actualVersion))
                return Fail(context, operand, ReasonStrings.VersionConstraint, actualVersion, constraint);

            return Pass();
        }
        return Invalid(context, API_VERSION);
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

    private static bool PassPathNotFound(IExpressionContext context, string name)
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
    private static bool NullOrEmpty(IExpressionContext context, IOperand operand)
    {
        return Fail(context, operand, ReasonStrings.Assert_IsNullOrEmpty);
    }

    /// <summary>
    /// Reason: The value '{0}' is not a string.
    /// </summary>
    private static bool NotString(IExpressionContext context, IOperand operand)
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

    private static bool TryPropertyLong(IExpressionContext context, LanguageExpression.PropertyBag properties, string propertyName, out long? propertyValue)
    {
        propertyValue = null;
        if (!properties.TryGetValue(propertyName, out var value))
            return false;

        if (!ExpressionHelpers.TryLong(Value(context, value), true, out var l_value))
            return false;

        propertyValue = l_value;
        return true;
    }

    private static bool TryField(LanguageExpression.PropertyBag properties, out string? field)
    {
        return properties.TryGetString(FIELD, out field);
    }

    private static bool TryField(IExpressionContext context, LanguageExpression.PropertyBag properties, ITargetObject o, out IOperand? operand)
    {
        operand = null;
        if (!properties.TryGetString(FIELD, out var field))
            return false;

        if (ObjectHelper.GetPath(context, o.Value, field, caseSensitive: false, out object value))
            operand = Operand.FromPath(field, value);

        return operand != null || NotHasField(context, field);
    }

    private static bool TryName(IExpressionContext context, LanguageExpression.PropertyBag properties, ITargetObject o, out IOperand? operand)
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

    private static bool TryType(IExpressionContext context, LanguageExpression.PropertyBag properties, ITargetObject o, out IOperand? operand)
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

    private static bool TrySource(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand? operand)
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

    private static bool TryValue(IExpressionContext context, LanguageExpression.PropertyBag properties, out IOperand? operand)
    {
        operand = null;
        if (properties.TryGetValue(VALUE, out var value))
        {
            // TODO: Propagate path
            operand = Operand.FromValue(value);
        }
        return operand != null;
    }

    private static bool TryScope(IExpressionContext context, LanguageExpression.PropertyBag properties, ITargetObject o, out IOperand? operand)
    {
        operand = null;
        if (properties.TryGetString(SCOPE, out var svalue))
        {
            if (svalue != DOT || context?.Context?.LanguageScope == null)
                return Invalid(context, svalue);

            if (!context.Context.TryGetScope(o.Value, out var scope))
                return Invalid(context, svalue);

            operand = Operand.FromScope(scope);
        }
        return operand != null;
    }

    /// <summary>
    /// Unwrap a function delegate or a literal value.
    /// </summary>
    private static object? Value(IExpressionContext context, IOperand operand)
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
        return TryPropertyBoolOrDefault(properties, CASE_SENSITIVE, out caseSensitive, defaultValue);
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
    private static bool TryFieldNotExists(IExpressionContext context, ITargetObject o, LanguageExpression.PropertyBag properties)
    {
        return properties.TryGetString(FIELD, out var field) &&
            !ObjectHelper.GetPath(context, o.Value, field, caseSensitive: false, out object _);
    }

    private static bool TryOperand(IExpressionContext context, string name, ITargetObject o, LanguageExpression.PropertyBag properties, out IOperand? operand)
    {
        return TryField(context, properties, o, out operand) ||
            TryType(context, properties, o, out operand) ||
            TryName(context, properties, o, out operand) ||
            TrySource(context, properties, out operand) ||
            TryValue(context, properties, out operand) ||
            TryScope(context, properties, o, out operand) ||
            Invalid(context, name);
    }

    private static bool TryPropertyArray(LanguageExpression.PropertyBag properties, string propertyName, out Array? propertyValue)
    {
        if (properties.TryGetValue(propertyName, out var array) && array is Array arrayValue)
        {
            propertyValue = arrayValue;
            return true;
        }
        propertyValue = null;
        return false;
    }

    private static bool TryPropertyStringArray(LanguageExpression.PropertyBag properties, string propertyName, out string[]? propertyValue)
    {
        if (properties.TryGetStringArray(propertyName, out propertyValue))
        {
            return true;
        }
        else if (properties.TryGetString(propertyName, out var s))
        {
            propertyValue = [s];
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

    private static string StringJoinArray(Array propertyValue)
    {
        return StringJoin(propertyValue.OfType<object>());
    }

    private static string StringJoin<T>(IEnumerable<T> propertyValue)
    {
        return string.Concat("'", string.Join("', '", propertyValue), "'");
    }

    private static string StringJoinNormalizedPath(string[] path)
    {
        var normalizedPath = path.Select(p => ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), p));
        return StringJoin(normalizedPath);
    }

    #endregion Helper methods
}
