// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Net;
using Manatee.Json;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Runtime;

/// <summary>
/// A set of assertion helpers that are exposed at runtime through the $Assert variable.
/// </summary>
public sealed class Assert
{
    private const string COMMASEPARATOR = ", ";
    private const string PROPERTY_SCHEMA = "$schema";
    private const string VARIABLE_NAME = "Assert";
    private const string TYPENAME_STRING = "[string]";
    private const string TYPENAME_NULL = "null";

    #region Authoring

    /// <summary>
    /// Create a result based on a boolean <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">A boolean condition that passes when set to <c>true</c>, and fails when set to <c>false</c>.</param>
    /// <param name="reason">A localized reason why the assertion failed. This parameter is ignored if the assertion passed.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Create(bool condition, string? reason = null)
    {
        return Create(condition, reason, args: null);
    }

    /// <summary>
    /// Create a result based on a boolean <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">A boolean condition that passes when set to <c>true</c>, and fails when set to <c>false</c>.</param>
    /// <param name="reason">An unformatted localized reason why the assertion failed. This parameter is ignored if the assertion passed.</param>
    /// <param name="args">A list of arguments that are inserted into the format string.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Create(bool condition, string reason, params object[] args)
    {
        return Create(Operand.FromTarget(), condition, reason, args);
    }

    /// <summary>
    /// Create a result based on a boolean <paramref name="condition"/>.
    /// </summary>
    /// <param name="path">The object path that was reported by the assertion.</param>
    /// <param name="condition">A boolean condition that passes when set to <c>true</c>, and fails when set to <c>false</c>.</param>
    /// <param name="reason">An unformatted localized reason why the assertion failed. This parameter is ignored if the assertion passed.</param>
    /// <param name="args">A list of arguments that are inserted into the format string.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Create(string path, bool condition, string reason, params object[] args)
    {
        return Create(string.IsNullOrEmpty(path) ? Operand.FromTarget() : Operand.FromPath(path), condition, reason, args);
    }

    internal AssertResult Create(IOperand operand, bool condition, string reason, params object[] args)
    {
        if (!(LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule) || LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Precondition)))
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VariableConditionScope, VARIABLE_NAME));

        return new AssertResult(operand, condition, reason, args);
    }

    /// <summary>
    /// Create a result based on issues reported downstream.
    /// </summary>
    /// <param name="issue">An array of issues reported downstream.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Create(TargetIssueInfo[] issue)
    {
        if (issue == null || issue.Length == 0)
            return Pass();

        var result = Fail();
        for (var i = 0; i < issue.Length; i++)
            result.AddReason(string.IsNullOrEmpty(issue[i].Path) ? Operand.FromTarget() : Operand.FromPath(issue[i].Path), issue[i].Message);

        return result;
    }

    /// <summary>
    /// Create a passing assertion result.
    /// </summary>
    /// <returns>An assertion result.</returns>
    public AssertResult Pass()
    {
        return Create(condition: true);
    }

    /// <summary>
    /// Create a failed assertion result.
    /// </summary>
    /// <returns>An assertion result.</returns>
    public AssertResult Fail()
    {
        return Create(condition: false, reason: null, args: null);
    }

    /// <summary>
    /// Create a failed assertion result.
    /// </summary>
    /// <param name="reason">An unformatted localized reason why the assertion failed.</param>
    /// <param name="args">A list of arguments that are inserted into the format string.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Fail(string reason, params object[] args)
    {
        return Create(condition: false, reason: reason, args: args);
    }

    /// <summary>
    /// Create a failed assertion result.
    /// </summary>
    /// <param name="operand">An operand that was reported by the assertion.</param>
    /// <param name="reason">An unformatted localized reason why the assertion failed.</param>
    /// <param name="args">A list of arguments that are inserted into the format string.</param>
    /// <returns>An assertion result.</returns>
    public AssertResult Fail(IOperand operand, string reason, params object[] args)
    {
        return Create(operand, condition: false, reason: reason, args: args);
    }

    #endregion Authoring

    #region Operators

    /// <summary>
    /// Aggregates one or more results. If any one results is a pass, then pass is returned.
    /// </summary>
    public AssertResult AnyOf(params AssertResult[] results)
    {
        if (results == null || results.Length == 0)
            return Fail(ReasonStrings.ResultsNotProvided);

        var result = Fail();
        for (var i = 0; i < results.Length; i++)
        {
            if (results[i].Result)
                return Pass();
            else
                result.AddReason(results[i]);
        }
        return result;
    }

    /// <summary>
    /// Aggregates one or more results. If all results are a pass, then pass is returned.
    /// </summary>
    public AssertResult AllOf(params AssertResult[] results)
    {
        if (results == null || results.Length == 0)
            return Fail(ReasonStrings.ResultsNotProvided);

        var result = Fail();
        var shouldPass = true;
        for (var i = 0; i < results.Length; i++)
        {
            if (!results[i].Result)
            {
                result.AddReason(results[i]);
                shouldPass = false;
            }
        }
        return shouldPass ? Pass() : result;
    }

    #endregion Operators

    #region Conditions

    /// <summary>
    /// The object should match the defined schema.
    /// </summary>
    public AssertResult JsonSchema(PSObject inputObject, string uri)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(uri, nameof(uri), out result))
            return result;

        // Get the schema
        if (!TryReadJson(uri, out var schemaContent))
            return Fail(ReasonStrings.JsonSchemaNotFound, uri);

        var s = new JsonSerializer();
        var schema = s.Deserialize<JsonSchema>(JsonValue.Parse(schemaContent));

        // Get the TargetObject
        var json = JsonValue.Parse(inputObject.ToJson());

        // Validate
        var schemaOptions = new JsonSchemaOptions
        {
            OutputFormat = SchemaValidationOutputFormat.Basic
        };
        var schemaResults = schema.Validate(json, schemaOptions);

        // Schema is valid
        if (schemaResults.IsValid)
            return Pass();

        // Handle schema invalid
        result = Fail();

        if (!string.IsNullOrEmpty(schemaResults.ErrorMessage))
            result.AddReason(Operand.FromTarget(), ReasonStrings.JsonSchemaInvalid, schemaResults.InstanceLocation.ToString(), schemaResults.ErrorMessage);

        foreach (var r in schemaResults.NestedResults)
            if (!string.IsNullOrEmpty(r.ErrorMessage))
                result.AddReason(Operand.FromTarget(), ReasonStrings.JsonSchemaInvalid, r.InstanceLocation.ToString(), r.ErrorMessage);

        return result;
    }

    /// <summary>
    /// The object should have the $schema property defined with the URI.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The field '$schema' does not exist.
    /// The field value '$schema' is not a string.
    /// The value of '$schema' is null or empty.
    /// None of the specified schemas match '{0}'.
    /// </remarks>
    public AssertResult HasJsonSchema(PSObject inputObject, string[]? uri = null, bool ignoreScheme = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardField(inputObject, PROPERTY_SCHEMA, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(PROPERTY_SCHEMA), fieldValue, out var actualSchema, out result))
            return result;

        if (string.IsNullOrEmpty(actualSchema))
            return Fail(Operand.FromPath(PROPERTY_SCHEMA), ReasonStrings.NotHasFieldValue, PROPERTY_SCHEMA);

        return uri == null || uri.Length == 0 || ExpressionHelpers.AnySchema(actualSchema, uri, ignoreScheme, false)
            ? Pass()
            : Fail(Operand.FromPath(PROPERTY_SCHEMA), ReasonStrings.Assert_NotSpecifiedSchema, actualSchema);
    }

    /// <summary>
    /// The object must have any of the specified fields.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// Does not exist.
    /// </remarks>
    public AssertResult HasField(PSObject inputObject, string[] field, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        result = Fail();
        for (var i = 0; field != null && i < field.Length; i++)
        {
            if (ExpressionHelpers.Exists(PipelineContext.CurrentThread, inputObject, field[i], caseSensitive))
                return Pass();

            result.AddReason(Operand.FromPath(field[i]), ReasonStrings.Assert_Exists);
        }
        return result;
    }

    /// <summary>
    /// The object must not have any of the specified fields.
    /// </summary>
    public AssertResult NotHasField(PSObject inputObject, string[] field, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        result = Pass();
        for (var i = 0; field != null && i < field.Length; i++)
        {
            if (ObjectHelper.GetPath(
                bindingContext: PipelineContext.CurrentThread,
                targetObject: inputObject,
                path: field[i],
                caseSensitive: caseSensitive,
                value: out object _))
            {
                if (result.Result)
                    result = Fail();

                result.AddReason(Operand.FromPath(field[i]), ReasonStrings.Assert_NotExists);
            }
        }
        return result;
    }

    /// <summary>
    /// The object must have all of the specified fields.
    /// </summary>
    public AssertResult HasFields(PSObject inputObject, string[] field, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        result = Fail();
        var missing = 0;
        for (var i = 0; field != null && i < field.Length; i++)
        {
            if (!ObjectHelper.GetPath(
                bindingContext: PipelineContext.CurrentThread,
                targetObject: inputObject,
                path: field[i],
                caseSensitive: caseSensitive,
                value: out object _))
            {
                result.AddReason(Operand.FromPath(field[i]), ReasonStrings.Assert_Exists);
                missing++;
            }
        }
        return missing == 0 ? Pass() : result;
    }

    /// <summary>
    /// The object should have a specific field with a value set.
    /// </summary>
    /// <remarks>
    /// Does not exist.
    /// Is null or empty.
    /// Is set to '{0}'.
    /// </remarks>
    public AssertResult HasFieldValue(PSObject inputObject, string field, object? expectedValue = null)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        // Assert
        if (!ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: false,
            value: out object fieldValue))
            return Fail(Operand.FromPath(field), ReasonStrings.Assert_Exists);
        else if (ExpressionHelpers.NullOrEmpty(fieldValue))
            return Fail(Operand.FromPath(field), ReasonStrings.Assert_IsNullOrEmpty);
        else if (expectedValue != null && !ExpressionHelpers.Equal(expectedValue, fieldValue, caseSensitive: false))
            return Fail(Operand.FromPath(field), ReasonStrings.Assert_IsSetTo, fieldValue);

        return Pass();
    }

    /// <summary>
    /// The object should not have the field or the field value is set to the default value.
    /// </summary>
    public AssertResult HasDefaultValue(PSObject inputObject, string field, object defaultValue)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        // Assert
        return !ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: false,
            value: out object fieldValue)
            || ExpressionHelpers.Equal(defaultValue, fieldValue, caseSensitive: false)
            ? Pass()
            : Fail(ReasonStrings.HasExpectedFieldValue, field, fieldValue);
    }

    /// <summary>
    /// The object field value must be null.
    /// </summary>
    public AssertResult Null(PSObject inputObject, string field)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: false,
            value: out object fieldValue);
        return fieldValue == null ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.NotNull, field);
    }

    /// <summary>
    /// The object field value must not be null.
    /// </summary>
    public AssertResult NotNull(PSObject inputObject, string field)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        return fieldValue == null ? Fail(Operand.FromPath(field), ReasonStrings.Null, field) : Pass();
    }

    /// <summary>
    /// The object should not have the field or the field value is null or empty.
    /// </summary>
    public AssertResult NullOrEmpty(PSObject inputObject, string field)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        // Assert
        return ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: false,
            value: out object fieldValue) && !ExpressionHelpers.NullOrEmpty(fieldValue)
            ? Fail(Operand.FromPath(field), ReasonStrings.NullOrEmpty, field)
            : Pass();
    }

    /// <summary>
    /// The value should start with the any of the specified prefixes. Only applies to strings.
    /// </summary>
    public AssertResult StartsWith(PSObject inputObject, string field, string[] prefix, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(prefix, nameof(prefix), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (prefix == null || prefix.Length == 0)
            return Pass();

        // Assert
        for (var i_prefix = 0; i_prefix < prefix.Length; i_prefix++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.StartsWith(value[i_value], prefix[i_prefix], caseSensitive))
                    return Pass();
            }
        }
        return Fail(Operand.FromPath(field), ReasonStrings.StartsWith, field, FormatArray(prefix));
    }

    /// <summary>
    /// The value should not start with the any of the specified prefixes. Only applies to strings.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// The parameter 'prefix' is null.
    /// The field '{0}' does not exist.
    /// The value '{0}' starts with '{1}'.
    /// </remarks>
    public AssertResult NotStartsWith(PSObject inputObject, string field, string[] prefix, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(prefix, nameof(prefix), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (prefix == null || prefix.Length == 0 || GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out _))
            return Pass();

        // Assert
        for (var i_prefix = 0; i_prefix < prefix.Length; i_prefix++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.StartsWith(value[i_value], prefix[i_prefix], caseSensitive))
                    return Fail(Operand.FromPath(field), ReasonStrings.Assert_StartsWith, value, prefix[i_prefix]);
            }
        }
        return Pass();
    }

    /// <summary>
    /// The value should end with the any of the specified suffix. Only applies to strings.
    /// </summary>
    public AssertResult EndsWith(PSObject inputObject, string field, string[] suffix, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(suffix, nameof(suffix), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (suffix == null || suffix.Length == 0)
            return Pass();

        // Assert
        for (var i_suffix = 0; i_suffix < suffix.Length; i_suffix++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.EndsWith(value[i_value], suffix[i_suffix], caseSensitive))
                    return Pass();
            }
        }
        return Fail(Operand.FromPath(field), ReasonStrings.EndsWith, field, FormatArray(suffix));
    }

    /// <summary>
    /// The value should not end with the any of the specified suffix. Only applies to strings.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// The parameter 'prefix' is null.
    /// The field '{0}' does not exist.
    /// The value '{0}' ends with '{1}'.
    /// </remarks>
    public AssertResult NotEndsWith(PSObject inputObject, string field, string[] suffix, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(suffix, nameof(suffix), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (suffix == null || suffix.Length == 0 || GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out _))
            return Pass();

        // Assert
        for (var i_suffix = 0; i_suffix < suffix.Length; i_suffix++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.EndsWith(value[i_value], suffix[i_suffix], caseSensitive))
                    return Fail(Operand.FromPath(field), ReasonStrings.Assert_EndsWith, value, suffix[i_suffix]);
            }
        }
        return Pass();
    }

    /// <summary>
    /// The value should contain with the any of the specified text. Only applies to strings.
    /// </summary>
    public AssertResult Contains(PSObject inputObject, string field, string[] text, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(text, nameof(text), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (text == null || text.Length == 0)
            return Pass();

        // Assert
        for (var i_text = 0; i_text < text.Length; i_text++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.Contains(value[i_value], text[i_text], caseSensitive))
                    return Pass();
            }
        }
        return Fail(Operand.FromPath(field), ReasonStrings.Contains, field, FormatArray(text));
    }

    /// <summary>
    /// The value should not contain with the any of the specified text. Only applies to strings.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// The parameter 'prefix' is null.
    /// The field '{0}' does not exist.
    /// The value '{0}' contains '{1}'.
    /// </remarks>
    public AssertResult NotContains(PSObject inputObject, string field, string[] text, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(text, nameof(text), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (text == null || text.Length == 0 || GuardStringOrArray(Operand.FromPath(field), fieldValue, out var value, out _))
            return Pass();

        // Assert
        for (var i_text = 0; i_text < text.Length; i_text++)
        {
            for (var i_value = 0; i_value < value.Length; i_value++)
            {
                if (ExpressionHelpers.Contains(value[i_value], text[i_text], caseSensitive))
                    return Fail(Operand.FromPath(field), ReasonStrings.Assert_Contains, value, text[i_text]);
            }
        }
        return Pass();
    }

    /// <summary>
    /// The object field value should only contain lowercase characters.
    /// </summary>
    public AssertResult IsLower(PSObject inputObject, string field, bool requireLetters = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (!ExpressionHelpers.IsLower(value, requireLetters, out var notLetters))
            return Fail(Operand.FromPath(field), notLetters ? ReasonStrings.IsLetter : ReasonStrings.Assert_IsLower, value);

        return Pass();
    }

    /// <summary>
    /// The object field value should only contain uppercase characters.
    /// </summary>
    public AssertResult IsUpper(PSObject inputObject, string field, bool requireLetters = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (!ExpressionHelpers.IsUpper(value, requireLetters, out var notLetters))
            return Fail(Operand.FromPath(field), notLetters ? ReasonStrings.IsLetter : ReasonStrings.Assert_IsUpper, value);

        return Pass();
    }

    /// <summary>
    /// The object field value should be a numeric type.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not numeric.
    /// </remarks>
    public AssertResult IsNumeric(PSObject inputObject, string field, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryInt(fieldValue, convert, out _) ||
            ExpressionHelpers.TryLong(fieldValue, convert, out _) ||
            ExpressionHelpers.TryFloat(fieldValue, convert, out _) ||
            ExpressionHelpers.TryByte(fieldValue, convert, out _) ||
            ExpressionHelpers.TryDouble(fieldValue, convert, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotNumeric, fieldValue);
    }

    /// <summary>
    /// The object field value should be an integer type.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not an integer.
    /// </remarks>
    public AssertResult IsInteger(PSObject inputObject, string field, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryInt(fieldValue, convert, out _) ||
            ExpressionHelpers.TryLong(fieldValue, convert, out _) ||
            ExpressionHelpers.TryByte(fieldValue, convert, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotInteger, fieldValue);
    }

    /// <summary>
    /// The object field value should be a boolean.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not a boolean.
    /// </remarks>
    public AssertResult IsBoolean(PSObject inputObject, string field, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryBool(fieldValue, convert, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotBoolean, fieldValue);
    }

    /// <summary>
    /// The object field value should be an array.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not an array
    /// </remarks>
    public AssertResult IsArray(PSObject inputObject, string field)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryArray(fieldValue, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotArray, fieldValue);
    }

    /// <summary>
    /// The object field value should be a string.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not a string.
    /// </remarks>
    public AssertResult IsString(PSObject inputObject, string field)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryString(fieldValue, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotString, fieldValue);
    }

    /// <summary>
    /// The object field value should be a DateTime.
    /// </summary>
    /// <remarks>
    /// The value '{0}' is not a date.
    /// </remarks>
    public AssertResult IsDateTime(PSObject inputObject, string field, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        return ExpressionHelpers.TryDateTime(fieldValue, convert, out _)
            ? Pass()
            : Fail(Operand.FromPath(field), ReasonStrings.Assert_NotDateTime, fieldValue);
    }

    /// <summary>
    /// The object field value should be one of the specified types.
    /// </summary>
    public AssertResult TypeOf(PSObject inputObject, string field, Type[] type)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullOrEmptyParam(type, nameof(type), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        result = Fail();
        for (var i = 0; type != null && i < type.Length; i++)
        {
            var o = ExpressionHelpers.GetBaseObject(fieldValue);
            if (type[i].IsAssignableFrom(fieldValue.GetType()) ||
                type[i].IsAssignableFrom(o.GetType()) ||
                TryTypeName(fieldValue, type[i].FullName))
                return Pass();

            result.AddReason(Operand.FromPath(field), ReasonStrings.Type, type[i].Name, GetTypeName(fieldValue), fieldValue);
        }
        return result;
    }

    /// <summary>
    /// The object field value should be one of the specified types.
    /// </summary>
    public AssertResult TypeOf(PSObject inputObject, string field, string[] type)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullOrEmptyParam(type, nameof(type), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardNullFieldValue(field, fieldValue, out result))
            return result;

        result = Fail();
        for (var i = 0; type != null && i < type.Length; i++)
        {
            var o = ExpressionHelpers.GetBaseObject(fieldValue);
            if (StringComparer.OrdinalIgnoreCase.Equals(fieldValue.GetType().FullName, type[i]) ||
                StringComparer.OrdinalIgnoreCase.Equals(o.GetType().FullName, type[i]) ||
                TryTypeName(fieldValue, type[i]))
                return Pass();

            result.AddReason(Operand.FromPath(field), ReasonStrings.Type, type[i], GetTypeName(fieldValue), fieldValue);
        }
        return result;
    }

    /// <summary>
    /// The Version assertion method checks the field value is a valid semantic version.
    /// A constraint can optionally be provided to require the semantic version to be within a range.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#version"/>
    /// </summary>
    /// <remarks>
    /// Only applies to strings.
    /// </remarks>
    public AssertResult Version(PSObject inputObject, string field, string? constraint = null, bool includePrerelease = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardSemanticVersion(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (!SemanticVersion.TryParseConstraint(constraint, out var c, includePrerelease))
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, value));

        // Assert
        return c != null && !c.Accepts(value) ? Fail(Operand.FromPath(field), ReasonStrings.VersionConstraint, value, constraint) : Pass();
    }

    /// <summary>
    /// The APIVersion assertion method checks the field value is a valid date version.
    /// A constraint can optionally be provided to require the date version to be within a range.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#apiversion"/>
    /// </summary>
    /// <remarks>
    /// Only applies to strings.
    /// </remarks>
    public AssertResult APIVersion(PSObject inputObject, string field, string? constraint = null, bool includePrerelease = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardDateVersion(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (!DateVersion.TryParseConstraint(constraint, out var c, includePrerelease))
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, value));

        // Assert
        return c != null && !c.Accepts(value) ? Fail(Operand.FromPath(field), ReasonStrings.VersionConstraint, value, constraint) : Pass();
    }

    /// <summary>
    /// The Greater assertion method checks the field value is greater than the specified value.
    /// The field value can either be an integer, float, array, or string.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#greater"/>
    /// </summary>
    public AssertResult Greater(PSObject inputObject, string field, int value, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
            return compare > 0 ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.Greater, actual, value);

        return Fail(Operand.FromPath(field), ReasonStrings.Compare, fieldValue, value);
    }

    /// <summary>
    /// The GreaterOrEqual assertion method checks the field value is greater or equal to the specified value.
    /// The field value can either be an integer, float, array, or string.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#greaterorequal"/>
    /// </summary>
    public AssertResult GreaterOrEqual(PSObject inputObject, string field, int value, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
            return compare >= 0 ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.GreaterOrEqual, actual, value);

        return Fail(Operand.FromPath(field), ReasonStrings.Compare, fieldValue, value);
    }

    /// <summary>
    /// The Less assertion method checks the field value is less than the specified value.
    /// The field value can either be an integer, float, array, or string.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#less"/>
    /// </summary>
    public AssertResult Less(PSObject inputObject, string field, int value, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
            return compare < 0 ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.Less, actual, value);

        return Fail(Operand.FromPath(field), ReasonStrings.Compare, fieldValue, value);
    }

    /// <summary>
    /// The LessOrEqual assertion method checks the field value is less or equal to the specified value.
    /// The field value can either be an integer, float, array, or string.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#lessorequal"/>
    /// </summary>
    public AssertResult LessOrEqual(PSObject inputObject, string field, int value, bool convert = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
            return compare <= 0 ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.LessOrEqual, actual, value);

        return Fail(Operand.FromPath(field), ReasonStrings.Compare, fieldValue, value);
    }

    /// <summary>
    /// The object field value must be included in the set.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#in"/>
    /// </summary>
    public AssertResult In(PSObject inputObject, string field, Array values, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(values, nameof(values), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        for (var i = 0; values != null && i < values.Length; i++)
        {
            if (ExpressionHelpers.AnyValue(fieldValue, values.GetValue(i), caseSensitive, out var _))
                return Pass();
        }
        return Fail(Operand.FromPath(field), ReasonStrings.In, fieldValue);
    }

    /// <summary>
    /// The object field value must not be included in the set.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#notin"/>
    /// </summary>
    public AssertResult NotIn(PSObject inputObject, string field, Array values, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(values, nameof(values), out result))
            return result;

        if (!ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: caseSensitive,
            value: out object fieldValue))
            return Pass();

        for (var i = 0; values != null && i < values.Length; i++)
        {
            if (ExpressionHelpers.AnyValue(fieldValue, values.GetValue(i), caseSensitive, out var foundValue))
                return Fail(Operand.FromPath(field), ReasonStrings.NotIn, foundValue);
        }
        return Pass();
    }

    /// <summary>
    /// The Subset assertion method checks the field value includes all of the specified values.
    /// The field value may also contain additional values that are not specified in the values parameter.
    /// The field value must be an array or collection.
    /// Specified values can be included in the field value in any order.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#subset"/>
    /// </summary>
    public AssertResult Subset(PSObject inputObject, string field, Array values, bool caseSensitive = false, bool unique = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(values, nameof(values), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardFieldEnumerable(fieldValue, field, out _, out result))
            return result;

        for (var i = 0; values != null && i < values.Length; i++)
        {
            if (!ExpressionHelpers.CountValue(fieldValue, values.GetValue(i), caseSensitive, out var count) || (count > 1 && unique))
                return count == 0 ? Fail(Operand.FromPath(field), ReasonStrings.Subset, field, values.GetValue(i)) : Fail(Operand.FromPath(field), ReasonStrings.SubsetDuplicate, field, values.GetValue(i));
        }
        return Pass();
    }

    /// <summary>
    /// The SetOf assertion method checks the field value only includes all of the specified values.
    /// The field value must be an array or collection.
    /// Specified values can be included in the field value in any order.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#setof"/>
    /// </summary>
    public AssertResult SetOf(PSObject inputObject, string field, Array values, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(values, nameof(values), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardFieldEnumerable(fieldValue, field, out var count, out result))
            return result;

        if (count != values.Length)
            return Fail(Operand.FromPath(field), ReasonStrings.Count, field, count, values.Length);

        for (var i = 0; values != null && i < values.Length; i++)
        {
            if (!ExpressionHelpers.AnyValue(fieldValue, values.GetValue(i), caseSensitive, out _))
                return Fail(Operand.FromPath(field), ReasonStrings.Subset, field, values.GetValue(i));
        }
        return Pass();
    }

    /// <summary>
    /// The field value must contain the specified number of items.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#count"/>
    /// </summary>
    public AssertResult Count(PSObject inputObject, string field, int count)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardFieldEnumerable(fieldValue, field, out var actual, out result))
            return result;

        return actual == count ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.Count, field, actual, count);
    }

    /// <summary>
    /// The field value must not contain the specified number of items.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#notcount"/>
    /// </summary>
    public AssertResult NotCount(PSObject inputObject, string field, int count)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardFieldEnumerable(fieldValue, field, out var actual, out result))
            return result;

        return actual != count ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.NotCount, field, actual, count);
    }

    /// <summary>
    /// The object field value must match the regular expression.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#match"/>
    /// </summary>
    public AssertResult Match(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        return ExpressionHelpers.Match(pattern, value, caseSensitive) ? Pass() : Fail(Operand.FromPath(field), ReasonStrings.MatchPattern, value, pattern);
    }

    /// <summary>
    /// The object field value must not match the regular expression.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#notmatch"/>
    /// </summary>
    public AssertResult NotMatch(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result))
            return result;

        if (!ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: caseSensitive,
            value: out object fieldValue))
            return Pass();

        if (GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (!ExpressionHelpers.Match(pattern, value, caseSensitive))
            return Pass();

        return Fail(Operand.FromPath(field), ReasonStrings.NotMatchPattern, value, pattern);
    }

    /// <summary>
    /// The FilePath assertion method checks the file exists.
    /// Checks use file system case-sensitivity rules.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#filepath"/>
    /// </summary>
    public AssertResult FilePath(PSObject inputObject, string field, string[]? suffix = null)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (suffix == null || suffix.Length == 0)
        {
            return !TryFilePath(value, out _) ? Fail(Operand.FromPath(field), ReasonStrings.FilePath, value) : Pass();
        }

        var reason = Fail();
        for (var i = 0; i < suffix.Length; i++)
        {
            if (!TryFilePath(Path.Combine(value, suffix[i]), out _))
                reason.AddReason(Operand.FromPath(field), ReasonStrings.FilePath, suffix[i]);
            else
                return Pass();
        }
        return reason;
    }

    /// <summary>
    /// The FileHeader assertion method checks a file for a comment header.
    /// When comparing the file header, the format of line comments are automatically detected by file extension.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Assert/#fileheader"/>
    /// </summary>
    public AssertResult FileHeader(PSObject inputObject, string field, string[] header, string? prefix = null)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        // File does not exist
        if (!TryFilePath(value, out _))
            return Fail(Operand.FromPath(field), ReasonStrings.FilePath, value);

        // No header
        if (header == null || header.Length == 0)
            return Pass();

        if (string.IsNullOrEmpty(prefix))
            prefix = DetectLinePrefix(GetFileType(value));

        var lineNo = 0;
        foreach (var content in File.ReadLines(value))
        {
            if (lineNo >= header.Length)
                break;

            if (content != string.Concat(prefix, header[lineNo]))
                return Fail(Operand.FromPath(field), ReasonStrings.FileHeader);

            lineNo++;
        }

        // Catch file has less lines than header
        return lineNo < header.Length ? Fail(Operand.FromPath(field), ReasonStrings.FileHeader) : Pass();
    }

    /// <summary>
    /// The field value must be within the specified path.
    /// </summary>
    public AssertResult WithinPath(PSObject inputObject, string field, string[] path, bool? caseSensitive = null)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullOrEmptyParam(path, nameof(path), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        var fieldValuePath = ExpressionHelpers.GetObjectOriginPath(fieldValue);
        result = Fail();
        for (var i = 0; path != null && i < path.Length; i++)
        {
            if (ExpressionHelpers.WithinPath(fieldValuePath, path[i], caseSensitive.GetValueOrDefault(PSRuleOption.IsCaseSensitive())))
                return Pass();

            result.AddReason(Operand.FromPath(field), ReasonStrings.WithinPath,
                ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), fieldValuePath),
                ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), path[i])
            );
        }
        return result;
    }

    /// <summary>
    /// The field must not be within the specified path.
    /// </summary>
    public AssertResult NotWithinPath(PSObject inputObject, string field, string[] path, bool? caseSensitive = null)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullOrEmptyParam(path, nameof(path), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        var fieldValuePath = ExpressionHelpers.GetObjectOriginPath(fieldValue);
        for (var i = 0; path != null && i < path.Length; i++)
        {
            if (ExpressionHelpers.WithinPath(fieldValuePath, path[i], caseSensitive.GetValueOrDefault(PSRuleOption.IsCaseSensitive())))
                return Fail(Operand.FromPath(field), ReasonStrings.NotWithinPath,
                    ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), fieldValuePath),
                    ExpressionHelpers.NormalizePath(Environment.GetWorkingPath(), path[i])
                );
        }
        return Pass();
    }

    /// <summary>
    /// The value should match at least one of the specified patterns. Only applies to strings.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// The parameter 'pattern' is null.
    /// The field '{0}' does not exist.
    /// The value '{0}' is not like '{1}'.
    /// </remarks>
    public AssertResult Like(PSObject inputObject, string field, string[] pattern, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(pattern, nameof(pattern), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result) ||
            GuardString(Operand.FromPath(field), fieldValue, out var value, out result))
            return result;

        if (pattern == null || pattern.Length == 0)
            return Pass();

        // Assert
        for (var i = 0; i < pattern.Length; i++)
        {
            if (ExpressionHelpers.Like(value, pattern[i], caseSensitive))
                return Pass();
        }
        return Fail(Operand.FromPath(field), ReasonStrings.Assert_NotLike, field, FormatArray(pattern));
    }

    /// <summary>
    /// The value should not match any of the specified patterns. Only applies to strings.
    /// </summary>
    /// <remarks>
    /// The parameter 'inputObject' is null.
    /// The parameter 'field' is null or empty.
    /// The parameter 'pattern' is null.
    /// The field '{0}' does not exist.
    /// The value '{0}' is like '{1}'.
    /// </remarks>
    public AssertResult NotLike(PSObject inputObject, string field, string[] pattern, bool caseSensitive = false)
    {
        // Guard parameters
        if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
            GuardNullOrEmptyParam(field, nameof(field), out result) ||
            GuardNullParam(pattern, nameof(pattern), out result) ||
            GuardField(inputObject, field, false, out var fieldValue, out result))
            return result;

        if (pattern == null || pattern.Length == 0 || GuardString(Operand.FromPath(field), fieldValue, out var value, out _))
            return Pass();

        // Assert
        for (var i = 0; i < pattern.Length; i++)
        {
            if (ExpressionHelpers.Like(value, pattern[i], caseSensitive))
                return Fail(Operand.FromPath(field), ReasonStrings.Assert_Like, value, pattern[i]);
        }
        return Pass();
    }

    #endregion Conditions

    #region Helper methods

    /// <summary>
    /// Fails if the value is null.
    /// </summary>
    /// <remarks>
    /// Reason: The parameter '{0}' is null.
    /// </remarks>
    private bool GuardNullParam(object value, string parameterName, out AssertResult result)
    {
        result = value == null ? Fail(ReasonStrings.NullParameter, parameterName) : null;
        return result != null;
    }

    /// <summary>
    /// Fails of the value is null or empty.
    /// </summary>
    /// <returns>Returns true if the value is null or an empty string.</returns>
    /// <remarks>
    /// Reason: The parameter '{0}' is null or empty.
    /// </remarks>
    private bool GuardNullOrEmptyParam(string value, string parameterName, out AssertResult result)
    {
        result = string.IsNullOrEmpty(value) ? Fail(ReasonStrings.NullOrEmptyParameter, parameterName) : null;
        return result != null;
    }

    /// <summary>
    /// Fails of the value is null or empty.
    /// </summary>
    /// <returns>Returns true if the value is null or is empty.</returns>
    /// <remarks>
    /// Reason: The parameter '{0}' is null or empty.
    /// </remarks>
    private bool GuardNullOrEmptyParam(Array value, string parameterName, out AssertResult result)
    {
        result = value == null || value.Length == 0 ? Fail(ReasonStrings.NullOrEmptyParameter, parameterName) : null;
        return result != null;
    }

    /// <summary>
    /// Fails if the field does not exist.
    /// </summary>
    /// <returns>Returns true if the field does not exist.</returns>
    /// <remarks>
    /// Reason: The field '{0}' does not exist.
    /// </remarks>
    private bool GuardField(PSObject inputObject, string field, bool caseSensitive, out object? fieldValue, out AssertResult? result)
    {
        result = null;
        if (ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: inputObject,
            path: field,
            caseSensitive: caseSensitive,
            value: out fieldValue))
            return false;

        result = Fail(Operand.FromPath(field), ReasonStrings.Assert_Exists, field);
        return true;
    }

    private bool GuardSemanticVersion(IOperand operand, object fieldValue, out SemanticVersion.Version? value, out AssertResult? result)
    {
        result = null;
        value = null;
        if (ExpressionHelpers.TryString(fieldValue, out var s) && SemanticVersion.TryParseVersion(s, out value))
            return false;

        result = Fail(operand, ReasonStrings.Version, fieldValue);
        return true;
    }

    private bool GuardDateVersion(IOperand operand, object fieldValue, out DateVersion.Version? value, out AssertResult? result)
    {
        result = null;
        value = null;
        if (ExpressionHelpers.TryString(fieldValue, out var s) && DateVersion.TryParseVersion(s, out value))
            return false;

        result = Fail(operand, ReasonStrings.Version, fieldValue);
        return true;
    }

    /// <summary>
    /// Fails if the field is not enumerable.
    /// </summary>
    /// <returns>Returns true of the field value is not enumerable.</returns>
    /// <remarks>
    /// Reason: The field '{0}' is not enumerable.
    /// </remarks>
    private bool GuardFieldEnumerable(object fieldValue, string field, out int count, out AssertResult? result)
    {
        result = null;
        if (ExpressionHelpers.TryEnumerableLength(fieldValue, out count))
            return false;

        result = Fail(Operand.FromPath(field), ReasonStrings.NotEnumerable, field);
        return true;
    }

    /// <summary>
    /// Fails if the field value is not a string.
    /// </summary>
    /// <returns>Returns true if the field value is not a string.</returns>
    /// <remarks>
    /// Reason: The field value '{0}' is not a string.
    /// </remarks>
    private bool GuardString(IOperand operand, object fieldValue, out string? value, out AssertResult? result)
    {
        result = null;
        if (ExpressionHelpers.TryString(fieldValue, out value))
            return false;

        result = Fail(operand, ReasonStrings.Type, TYPENAME_STRING, GetTypeName(fieldValue), fieldValue);
        return true;
    }

    /// <summary>
    /// Fails if the field value is not a string or an array of strings.
    /// </summary>
    /// <returns>Returns true if the field value is not a string or an array of strings.</returns>
    /// <remarks>
    /// Reason: The field value '{0}' is not a string.
    /// </remarks>
    private bool GuardStringOrArray(IOperand operand, object fieldValue, out string[]? value, out AssertResult? result)
    {
        result = null;
        if (ExpressionHelpers.TryStringOrArray(fieldValue, convert: false, value: out value))
            return false;

        result = Fail(operand, ReasonStrings.Type, TYPENAME_STRING, GetTypeName(fieldValue), fieldValue);
        return true;
    }

    /// <summary>
    /// Fields if the field value is null.
    /// </summary>
    /// <returns>Returns true if the field value is null.</returns>
    /// <remarks>
    /// Reason: The field value '{0}' is null.
    /// </remarks>
    private bool GuardNullFieldValue(string field, object? fieldValue, out AssertResult? result)
    {
        result = null;
        if (fieldValue != null)
            return false;

        result = Fail(Operand.FromPath(field), ReasonStrings.Null, field);
        return true;
    }

    private static bool TryReadJson(string uri, out string? json)
    {
        json = null;
        if (uri == null)
        {
            return false;
        }
        else if (uri.IsURL())
        {
            using var webClient = new WebClient();
            json = webClient.DownloadString(uri);
            return true;
        }
        else if (TryFilePath(uri, out var path))
        {
            using var reader = new StreamReader(path);
            json = reader.ReadToEnd();
            return true;
        }
        return false;
    }

    private static bool TryFilePath(string path, out string rootedPath)
    {
        rootedPath = Environment.GetRootedPath(path);
        return File.Exists(rootedPath);
    }

    private static string FormatArray(string[] values)
    {
        return string.Join(COMMASEPARATOR, values);
    }

    /// <summary>
    /// Get the file extension of the name of the file if an extension is not set.
    /// </summary>
    private static string GetFileType(string value)
    {
        var ext = Path.GetExtension(value);
        return string.IsNullOrEmpty(ext) ? Path.GetFileNameWithoutExtension(value) : ext;
    }

    /// <summary>
    /// Determine line comment prefix by file extension
    /// </summary>
    private static string DetectLinePrefix(string extension)
    {
        extension = extension?.ToLower();
        switch (extension)
        {
            case ".bicep":
            case ".bicepparam":
            case ".cs":
            case ".csx":
            case ".ts":
            case ".tsp":
            case ".tsx":
            case ".js":
            case ".jsx":
            case ".fs":
            case ".go":
            case ".groovy":
            case ".php":
            case ".cpp":
            case ".h":
            case ".java":
            case ".json":
            case ".jsonc":
            case ".scala":
            case "jenkinsfile":
                return "// ";

            case ".editorconfig":
            case ".ipynb":
            case ".ps1":
            case ".psd1":
            case ".psm1":
            case ".yaml":
            case ".yml":
            case ".r":
            case ".py":
            case ".sh":
            case ".tf":
            case ".tfvars":
            case ".toml":
            case ".gitignore":
            case ".pl":
            case ".rb":
            case "dockerfile":
                return "# ";

            case ".sql":
            case ".lua":
                return "-- ";

            case ".bat":
            case ".cmd":
                return ":: ";

            default:
                return string.Empty;
        }
    }

    private static string GetTypeName(object value)
    {
        return value == null ? TYPENAME_NULL : value.GetType().Name;
    }

    private static bool TryTypeName(object fieldValue, string typeName)
    {
        if (fieldValue is not PSObject pso)
            return false;

        for (var i = 0; i < pso.TypeNames.Count; i++)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(pso.TypeNames[i], typeName))
                return true;
        }
        return false;
    }

    #endregion Helper methods
}
