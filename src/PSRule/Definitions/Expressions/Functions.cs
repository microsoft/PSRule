// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Common;
using PSRule.Resources;
using PSRule.Runtime;
using static PSRule.Definitions.Expressions.LanguageExpression;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// Implementation of Azure Resource Manager template functions as ExpressionFn.
/// </summary>
internal static class Functions
{
    private const string BOOLEAN = "boolean";
    private const string STRING = "string";
    private const string INTEGER = "integer";
    private const string CONCAT = "concat";
    private const string SUBSTRING = "substring";
    private const string CONFIGURATION = "configuration";
    private const string PATH = "path";
    private const string LENGTH = "length";
    private const string REPLACE = "replace";
    private const string TRIM = "trim";
    private const string FIRST = "first";
    private const string LAST = "last";
    private const string SPLIT = "split";
    private const string PADLEFT = "padLeft";
    private const string PADRIGHT = "padRight";
    private const string DELIMITER = "delimiter";
    private const string OLDSTRING = "oldstring";
    private const string NEWSTRING = "newstring";
    private const string CASESENSITIVE = "casesensitive";
    private const string TOTALLENGTH = "totalLength";
    private const string PADDINGCHARACTER = "paddingCharacter";

    private const char SPACE = ' ';

    /// <summary>
    /// The available built-in functions.
    /// </summary>
    internal static readonly IFunctionDescriptor[] Builtin = new IFunctionDescriptor[]
    {
        new FunctionDescriptor(CONFIGURATION, Configuration),
        new FunctionDescriptor(PATH, Path),
        new FunctionDescriptor(BOOLEAN, Boolean),
        new FunctionDescriptor(STRING, String),
        new FunctionDescriptor(INTEGER, Integer),
        new FunctionDescriptor(CONCAT, Concat),
        new FunctionDescriptor(SUBSTRING, Substring),
        new FunctionDescriptor(REPLACE, Replace),
        new FunctionDescriptor(TRIM, Trim),
        new FunctionDescriptor(FIRST, First),
        new FunctionDescriptor(LAST, Last),
        new FunctionDescriptor(SPLIT, Split),
        new FunctionDescriptor(PADLEFT, PadLeft),
        new FunctionDescriptor(PADRIGHT, PadRight),
    };

    private static ExpressionFnOuter Boolean(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, BOOLEAN, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            ExpressionHelpers.TryBool(value, true, out var b);
            return b;
        };
    }

    private static ExpressionFnOuter String(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, STRING, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            ExpressionHelpers.TryString(value, true, out var s);
            return s;
        };
    }

    private static ExpressionFnOuter Integer(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, INTEGER, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            ExpressionHelpers.TryInt(value, true, out var i);
            return i;
        };
    }

    private static ExpressionFnOuter Configuration(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null || properties.Count == 0 ||
            !properties.TryGetString(CONFIGURATION, out var name))
            return null;

        // Lookup a configuration value.
        return (context) =>
        {
            return context.Context.TryGetConfigurationValue(name, out var value) ? value : null;
        };
    }

    private static ExpressionFnOuter Path(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null || properties.Count == 0 ||
            !properties.TryGetString(PATH, out var path))
            return null;

        return (context) =>
        {
            return ObjectHelper.GetPath(context, context.Current.Value, path, false, out object value) ? value : null;
        };
    }

    private static ExpressionFnOuter? Concat(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !properties.TryGetEnumerable(CONCAT, out var values))
            return null;

        return (context) =>
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                sb.Append(Value(context, value));
            }
            return sb.ToString();
        };
    }

    private static ExpressionFnOuter? Substring(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, LENGTH, out int? length) ||
            !TryProperty(properties, SUBSTRING, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            if (value is string s)
            {
                length = s.Length < length ? s.Length : length;
                return s.Substring(0, length.Value);
            }
            return null;
        };
    }

    private static ExpressionFnOuter Replace(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !properties.TryGetString(OLDSTRING, out var oldString) ||
            !properties.TryGetString(NEWSTRING, out var newString) ||
            !TryProperty(properties, REPLACE, out ExpressionFnOuter next))
            return null;

        var caseSensitive = properties.TryGetBool(CASESENSITIVE, out var cs) && cs.HasValue && cs.Value;
        return (context) =>
        {
            var value = next(context);
            if (ExpressionHelpers.TryString(value, out var originalString))
                return originalString.Length > 0 && oldString.Length > 0 ? originalString.Replace(oldString, newString, caseSensitive) : originalString;

            return null;
        };
    }

    private static ExpressionFnOuter Trim(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, TRIM, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            return ExpressionHelpers.TryString(value, out var s) ? s.Trim() : null;
        };
    }

    private static ExpressionFnOuter First(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, FIRST, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            if (ExpressionHelpers.TryString(value, out var s))
                return s.Length > 0 ? new string(s[0], 1) : null;

            return ExpressionHelpers.TryArray(value, out var array) ? Value(context, array.First()) : null;
        };
    }

    private static ExpressionFnOuter Last(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, LAST, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            if (ExpressionHelpers.TryString(value, out var s))
                return s.Length > 0 ? new string(s[s.Length - 1], 1) : null;

            return ExpressionHelpers.TryArray(value, out var array) ? Value(context, array.Last()) : null;
        };
    }

    private static ExpressionFnOuter Split(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !properties.TryGetStringArray(DELIMITER, out var delimiter) ||
            !TryProperty(properties, SPLIT, out ExpressionFnOuter next))
            return null;

        return (context) =>
        {
            var value = next(context);
            return ExpressionHelpers.TryString(value, out var s) ? s.Split(delimiter, options: StringSplitOptions.None) : null;
        };
    }

    private static ExpressionFnOuter PadLeft(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||

            !TryProperty(properties, PADLEFT, out ExpressionFnOuter next))
            return null;

        var paddingChar = properties.TryGetChar(PADDINGCHARACTER, out var c) ? c : SPACE;
        var totalWidth = properties.TryGetInt(TOTALLENGTH, out var i) ? i : 0;
        return (context) =>
        {
            var value = next(context);
            if (ExpressionHelpers.TryString(value, convert: true, value: out var s))
                return totalWidth > s.Length ? s.PadLeft(totalWidth.Value, paddingChar.Value) : s;

            return null;
        };
    }

    private static ExpressionFnOuter PadRight(IExpressionContext context, PropertyBag properties)
    {
        if (properties == null ||
            properties.Count == 0 ||
            !TryProperty(properties, PADRIGHT, out ExpressionFnOuter next))
            return null;

        var paddingChar = properties.TryGetChar(PADDINGCHARACTER, out var c) ? c : SPACE;
        var totalWidth = properties.TryGetInt(TOTALLENGTH, out var i) ? i : 0;
        return (context) =>
        {
            var value = next(context);
            if (ExpressionHelpers.TryString(value, convert: true, value: out var s))
                return totalWidth > s.Length ? s.PadRight(totalWidth.Value, paddingChar.Value) : s;

            return null;
        };
    }

    #region Helper functions

    private static bool TryProperty(PropertyBag properties, string name, out int? value)
    {
        return properties.TryGetInt(name, out value);
    }

    private static bool TryProperty(PropertyBag properties, string name, out ExpressionFnOuter value)
    {
        value = null;
        if (properties.TryGetValue(name, out var v) && v is ExpressionFnOuter fn)
            value = fn;

        else if (properties.TryGetBool(name, out var b_value))
            value = (context) => b_value;

        else if (properties.TryGetLong(name, out var l_value))
            value = (context) => l_value;

        else if (properties.TryGetInt(name, out var i_value))
            value = (context) => i_value;

        else if (properties.TryGetValue(name, out var o_value))
            value = (context) => o_value;

        return value != null;
    }

    private static object Value(IExpressionContext context, object value)
    {
        return value is ExpressionFnOuter fn ? fn(context) : value;
    }

    #endregion Helper functions

    #region Exceptions

    private static ExpressionArgumentException ArgumentsOutOfRange(string expression, object[] args)
    {
        var length = args == null ? 0 : args.Length;
        return new ExpressionArgumentException(
            expression,
            string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ArgumentsOutOfRange, expression, length)
        );
    }

    private static ExpressionArgumentException ArgumentFormatInvalid(string expression)
    {
        return new ExpressionArgumentException(
            expression,
            string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ArgumentFormatInvalid, expression)
        );
    }

    private static ExpressionArgumentException ArgumentInvalidInteger(string expression, string operand)
    {
        return new ExpressionArgumentException(
            expression,
            string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ArgumentInvalidInteger, operand, expression)
        );
    }

    private static ExpressionArgumentException ArgumentInvalidBoolean(string expression, string operand)
    {
        return new ExpressionArgumentException(
            expression,
            string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ArgumentInvalidBoolean, operand, expression)
        );
    }

    private static ExpressionArgumentException ArgumentInvalidString(string expression, string operand)
    {
        return new ExpressionArgumentException(
            expression,
            string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ArgumentInvalidString, operand, expression)
        );
    }

    #endregion Exceptions
}
