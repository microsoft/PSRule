// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Threading;
using Manatee.Json;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper variable exposed at runtime for rules.
    /// </summary>
    public sealed class Assert
    {
        private const string COMMASEPARATOR = ", ";
        private const string PROPERTY_SCHEMA = "$schema";
        private const string VARIABLE_NAME = "Assert";
        private const string TYPENAME_STRING = "[string]";
        private const string TYPENAME_BOOL = "[bool]";
        private const string TYPENAME_ARRAY = "[array]";
        private const string TYPENAME_NULL = "null";
        private const string TYPENAME_DATETIME = "[DateTime]";

        public AssertResult Create(bool condition, string reason = null)
        {
            return Create(condition: condition, reason: reason, args: null);
        }

        public AssertResult Create(bool condition, string reason, params object[] args)
        {
            if (!(RunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule) || RunspaceContext.CurrentThread.IsScope(RunspaceScope.Precondition)))
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VariableConditionScope, VARIABLE_NAME));

            return new AssertResult(this, condition, reason, args);
        }

        public AssertResult Create(TargetIssueInfo[] issue)
        {
            if (issue == null || issue.Length == 0)
                return Pass();

            var result = Fail();
            for (var i = 0; i < issue.Length; i++)
                result.AddReason(issue[i].Message);

            return result;
        }

        /// <summary>
        /// Pass the assertion.
        /// </summary>
        public AssertResult Pass()
        {
            return Create(condition: true);
        }

        /// <summary>
        /// Fail the assertion.
        /// </summary>
        public AssertResult Fail()
        {
            return Create(condition: false, reason: null, args: null);
        }

        /// <summary>
        /// Fail the assertion.
        /// </summary>
        /// <param name="reason">An unformatted reason why the assertion failed.</param>
        /// <param name="args">Additional parameters for the reason format.</param>
        public AssertResult Fail(string reason, params object[] args)
        {
            return Create(condition: false, reason: reason, args: args);
        }

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
                    result.AddReason(result);
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
                result.AddReason(ReasonStrings.JsonSchemaInvalid, schemaResults.InstanceLocation.ToString(), schemaResults.ErrorMessage);

            foreach (var r in schemaResults.NestedResults)
                if (!string.IsNullOrEmpty(r.ErrorMessage))
                    result.AddReason(ReasonStrings.JsonSchemaInvalid, r.InstanceLocation.ToString(), r.ErrorMessage);

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
        public AssertResult HasJsonSchema(PSObject inputObject, string[] uri = null, bool ignoreScheme = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardField(inputObject, PROPERTY_SCHEMA, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var actualSchema, out result))
                return result;

            if (string.IsNullOrEmpty(actualSchema))
                return Fail(ReasonStrings.NotHasFieldValue, PROPERTY_SCHEMA);

            return uri == null || uri.Length == 0 || ExpressionHelpers.AnySchema(actualSchema, uri, ignoreScheme, false)
                ? Pass()
                : Fail(ReasonStrings.Assert_NotSpecifiedSchema, actualSchema);
        }

        /// <summary>
        /// The object must have any of the specified fields.
        /// </summary>
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

                result.AddReason(ReasonStrings.NotHasField, field[i]);
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
                if (ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field[i], caseSensitive: caseSensitive, value: out object _))
                {
                    if (result.Result)
                        result = Fail();

                    result.AddReason(ReasonStrings.HasField, field[i]);
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
                if (!ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field[i], caseSensitive: caseSensitive, value: out object _))
                {
                    result.AddReason(ReasonStrings.NotHasField, field[i]);
                    missing++;
                }
            }
            return missing == 0 ? Pass() : result;
        }

        /// <summary>
        /// The object should have a specific field with a value set.
        /// </summary>
        public AssertResult HasFieldValue(PSObject inputObject, string field, object expectedValue = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            // Assert
            if (!ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: false, value: out object fieldValue))
                return Fail(ReasonStrings.NotHasField, field);
            else if (ExpressionHelpers.NullOrEmpty(fieldValue))
                return Fail(ReasonStrings.NotHasFieldValue, field);
            else if (expectedValue != null && !ExpressionHelpers.Equal(expectedValue, fieldValue, caseSensitive: false))
                return Fail(ReasonStrings.HasExpectedFieldValue, field, fieldValue);

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

            ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: false, value: out object fieldValue);
            return fieldValue == null ? Pass() : Fail(ReasonStrings.NotNull, field);
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

            return fieldValue == null ? Fail(ReasonStrings.Null, field) : Pass();
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
            if (ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: false, value: out object fieldValue) && !ExpressionHelpers.NullOrEmpty(fieldValue))
                return Fail(ReasonStrings.NullOrEmpty, field);

            return Pass();
        }

        /// <summary>
        /// The object field value should start with the any of the specified prefixes. Only applies to strings.
        /// </summary>
        public AssertResult StartsWith(PSObject inputObject, string field, string[] prefix, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardNullParam(prefix, nameof(prefix), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            if (prefix == null || prefix.Length == 0)
                return Pass();

            // Assert
            for (var i = 0; i < prefix.Length; i++)
            {
                if (ExpressionHelpers.StartsWith(value, prefix[i], caseSensitive))
                    return Pass();
            }
            return Fail(ReasonStrings.StartsWith, field, FormatArray(prefix));
        }

        /// <summary>
        /// The object field value should end with the any of the specified suffix. Only applies to strings.
        /// </summary>
        public AssertResult EndsWith(PSObject inputObject, string field, string[] suffix, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardNullParam(suffix, nameof(suffix), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            if (suffix == null || suffix.Length == 0)
                return Pass();

            // Assert
            for (var i = 0; i < suffix.Length; i++)
            {
                if (ExpressionHelpers.EndsWith(value, suffix[i], caseSensitive))
                    return Pass();
            }
            return Fail(ReasonStrings.EndsWith, field, FormatArray(suffix));
        }

        /// <summary>
        /// The object field value should contain with the any of the specified text. Only applies to strings.
        /// </summary>
        public AssertResult Contains(PSObject inputObject, string field, string[] text, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardNullParam(text, nameof(text), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            if (text == null || text.Length == 0)
                return Pass();

            // Assert
            for (var i = 0; i < text.Length; i++)
            {
                if (ExpressionHelpers.Contains(value, text[i], caseSensitive))
                    return Pass();
            }
            return Fail(ReasonStrings.Contains, field, FormatArray(text));
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
                GuardString(fieldValue, out var value, out result))
                return result;

            if (!ExpressionHelpers.IsLower(value, requireLetters, out var notLetters))
                return Fail(notLetters ? ReasonStrings.IsLetter : ReasonStrings.Assert_IsLower, value);

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
                GuardString(fieldValue, out var value, out result))
                return result;

            if (!ExpressionHelpers.IsUpper(value, requireLetters, out var notLetters))
                return Fail(notLetters ? ReasonStrings.IsLetter : ReasonStrings.Assert_IsUpper, value);

            return Pass();
        }

        /// <summary>
        /// The object field value should be a numeric type.
        /// </summary>
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
                : Fail(ReasonStrings.TypeNumeric, GetTypeName(fieldValue), fieldValue);
        }

        /// <summary>
        /// The object field value should be an integer type.
        /// </summary>
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
                : Fail(ReasonStrings.TypeInteger, GetTypeName(fieldValue), fieldValue);
        }

        /// <summary>
        /// The object field value should be a boolean.
        /// </summary>
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
                : Fail(ReasonStrings.Type, TYPENAME_BOOL, GetTypeName(fieldValue), fieldValue);
        }

        /// <summary>
        /// The object field value should be an array.
        /// </summary>
        public AssertResult IsArray(PSObject inputObject, string field)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardNullFieldValue(field, fieldValue, out result))
                return result;

            var o = GetBaseObject(fieldValue);
            return o is Array ? Pass() : Fail(ReasonStrings.Type, TYPENAME_ARRAY, GetTypeName(fieldValue), fieldValue);
        }

        /// <summary>
        /// The object field value should be a string.
        /// </summary>
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
                : Fail(ReasonStrings.Type, TYPENAME_STRING, GetTypeName(fieldValue), fieldValue);
        }

        /// <summary>
        /// The object field value should be a DateTime.
        /// </summary>
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
                : Fail(ReasonStrings.Type, TYPENAME_DATETIME, GetTypeName(fieldValue), fieldValue);
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
                var o = GetBaseObject(fieldValue);
                if (type[i].IsAssignableFrom(fieldValue.GetType()) || type[i].IsAssignableFrom(o.GetType()) || TryTypeName(fieldValue, type[i].FullName))
                    return Pass();

                result.AddReason(ReasonStrings.Type, type[i].Name, GetTypeName(fieldValue), fieldValue);
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
                var o = GetBaseObject(fieldValue);
                if (StringComparer.OrdinalIgnoreCase.Equals(fieldValue.GetType().FullName, type[i]) || StringComparer.OrdinalIgnoreCase.Equals(o.GetType().FullName, type[i]) || TryTypeName(fieldValue, type[i]))
                    return Pass();

                result.AddReason(ReasonStrings.Type, type[i], GetTypeName(fieldValue), fieldValue);
            }
            return result;
        }

        /// <summary>
        /// The object field value should match the version constraint. Only applies to strings.
        /// </summary>
        public AssertResult Version(PSObject inputObject, string field, string constraint = null, bool includePrerelease = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardSemanticVersion(fieldValue, out var value, out result))
                return result;

            if (!SemanticVersion.TryParseConstraint(constraint, out var c, includePrerelease))
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, value));

            // Assert
            return c != null && !c.Equals(value) ? Fail(ReasonStrings.VersionContraint, value, constraint) : Pass();
        }

        public AssertResult Greater(PSObject inputObject, string field, int value, bool convert = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result))
                return result;

            if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
                return compare > 0 ? Pass() : Fail(ReasonStrings.Greater, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult GreaterOrEqual(PSObject inputObject, string field, int value, bool convert = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result))
                return result;

            if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
                return compare >= 0 ? Pass() : Fail(ReasonStrings.GreaterOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult Less(PSObject inputObject, string field, int value, bool convert = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result))
                return result;

            if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
                return compare < 0 ? Pass() : Fail(ReasonStrings.Less, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult LessOrEqual(PSObject inputObject, string field, int value, bool convert = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result))
                return result;

            if (ExpressionHelpers.CompareNumeric(fieldValue, value, convert, out var compare, out var actual))
                return compare <= 0 ? Pass() : Fail(ReasonStrings.LessOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        /// <summary>
        /// The object field value must be included in the set.
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
            return Fail(ReasonStrings.In, fieldValue);
        }

        /// <summary>
        /// The object field value must not be included in the set.
        /// </summary>
        public AssertResult NotIn(PSObject inputObject, string field, Array values, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardNullParam(values, nameof(values), out result))
                return result;

            if (!ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: caseSensitive, value: out object fieldValue))
                return Pass();

            for (var i = 0; values != null && i < values.Length; i++)
            {
                if (ExpressionHelpers.AnyValue(fieldValue, values.GetValue(i), caseSensitive, out var foundValue))
                    return Fail(ReasonStrings.NotIn, foundValue);
            }
            return Pass();
        }

        /// <summary>
        /// The object field value must include the set.
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
                    return count == 0 ? Fail(ReasonStrings.Subset, field, values.GetValue(i)) : Fail(ReasonStrings.SubsetDuplicate, field, values.GetValue(i));
            }
            return Pass();
        }

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
                return Fail(ReasonStrings.Count, field, count, values.Length);

            for (var i = 0; values != null && i < values.Length; i++)
            {
                if (!ExpressionHelpers.AnyValue(fieldValue, values.GetValue(i), caseSensitive, out _))
                    return Fail(ReasonStrings.Subset, field, values.GetValue(i));
            }
            return Pass();
        }

        /// <summary>
        ///  The field value must contain the specified number of items.
        /// </summary>
        public AssertResult Count(PSObject inputObject, string field, int count)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardFieldEnumerable(fieldValue, field, out var actual, out result))
                return result;

            return actual == count ? Pass() : Fail(ReasonStrings.Count, field, actual, count);
        }

        /// <summary>
        /// The object field value must match the regular expression.
        /// </summary>
        public AssertResult Match(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            return ExpressionHelpers.Match(pattern, value, caseSensitive) ? Pass() : Fail(ReasonStrings.MatchPattern, value, pattern);
        }

        /// <summary>
        /// The object field value must not match the regular expression.
        /// </summary>
        public AssertResult NotMatch(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            if (!ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: caseSensitive, value: out object fieldValue))
                return Pass();

            if (GuardString(fieldValue, out var value, out result))
                return result;

            if (!ExpressionHelpers.Match(pattern, value, caseSensitive))
                return Pass();

            return Fail(ReasonStrings.NotMatchPattern, value, pattern);
        }

        public AssertResult FilePath(PSObject inputObject, string field, string[] suffix = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            if (suffix == null || suffix.Length == 0)
            {
                return !TryFilePath(value, out _) ? Fail(ReasonStrings.FilePath, value) : Pass();
            }

            var reason = Fail();
            for (var i = 0; i < suffix.Length; i++)
            {
                if (!TryFilePath(Path.Combine(value, suffix[i]), out _))
                    reason.AddReason(ReasonStrings.FilePath, suffix[i]);
                else
                    return Pass();
            }
            return reason;
        }

        public AssertResult FileHeader(PSObject inputObject, string field, string[] header, string prefix = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out var result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out var fieldValue, out result) ||
                GuardString(fieldValue, out var value, out result))
                return result;

            // File does not exist
            if (!TryFilePath(value, out _))
                return Fail(ReasonStrings.FilePath, value);

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
                    return Fail(ReasonStrings.FileHeader);

                lineNo++;
            }

            // Catch file has less lines than header
            return lineNo < header.Length ? Fail(ReasonStrings.FileHeader) : Pass();
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
                if (ExpressionHelpers.WithinPath(fieldValuePath, path[i], caseSensitive.GetValueOrDefault(PSRuleOption.IsCaseSentitive())))
                    return Pass();

                result.AddReason(ReasonStrings.WithinPath,
                    ExpressionHelpers.NormalizePath(PSRuleOption.GetWorkingPath(), fieldValuePath),
                    ExpressionHelpers.NormalizePath(PSRuleOption.GetWorkingPath(), path[i])
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
                if (ExpressionHelpers.WithinPath(fieldValuePath, path[i], caseSensitive.GetValueOrDefault(PSRuleOption.IsCaseSentitive())))
                    return Fail(ReasonStrings.NotWithinPath,
                        ExpressionHelpers.NormalizePath(PSRuleOption.GetWorkingPath(), fieldValuePath),
                        ExpressionHelpers.NormalizePath(PSRuleOption.GetWorkingPath(), path[i])
                    );
            }
            return Pass();
        }

        #region Helper methods

        /// <summary>
        /// Get the base object.
        /// </summary>
        private static object GetBaseObject(object o)
        {
            return o is PSObject pso && pso.BaseObject != null ? pso.BaseObject : o;
        }

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
        private bool GuardField(PSObject inputObject, string field, bool caseSensitive, out object fieldValue, out AssertResult result)
        {
            result = null;
            if (ObjectHelper.GetPath(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, path: field, caseSensitive: caseSensitive, value: out fieldValue))
                return false;

            result = Fail(ReasonStrings.NotHasField, field);
            return true;
        }

        private bool GuardSemanticVersion(object fieldValue, out SemanticVersion.Version value, out AssertResult result)
        {
            result = null;
            value = null;
            if (ExpressionHelpers.TryString(fieldValue, out var sversion) && Runtime.SemanticVersion.TryParseVersion(sversion, out value))
                return false;

            result = Fail(ReasonStrings.Version, fieldValue);
            return true;
        }

        /// <summary>
        /// Fails if the field is not enumerable.
        /// </summary>
        /// <returns>Returns true of the field value is not enumerable.</returns>
        /// <remarks>
        /// Reason: The field '{0}' is not enumerable.
        /// </remarks>
        private bool GuardFieldEnumerable(object fieldValue, string field, out int count, out AssertResult result)
        {
            result = null;
            if (ExpressionHelpers.TryEnumerableLength(fieldValue, out count))
                return false;

            result = Fail(ReasonStrings.NotEnumerable, field);
            return true;
        }

        /// <summary>
        /// Fails if the field value is not a string.
        /// </summary>
        /// <returns>Returns true if the field value is not a string.</returns>
        /// <remarks>
        /// Reason: The field value '{0}' is not a string.
        /// </remarks>
        private bool GuardString(object fieldValue, out string value, out AssertResult result)
        {
            result = null;
            if (ExpressionHelpers.TryString(fieldValue, out value))
                return false;

            result = Fail(ReasonStrings.Type, TYPENAME_STRING, GetTypeName(fieldValue), fieldValue);
            return true;
        }

        /// <summary>
        /// Fields if the field value is null.
        /// </summary>
        /// <returns>Returns true if the field value is null.</returns>
        /// <remarks>
        /// Reason: The field value '{0}' is null.
        /// </remarks>
        private bool GuardNullFieldValue(string field, object fieldValue, out AssertResult result)
        {
            result = null;
            if (fieldValue != null)
                return false;

            result = Fail(ReasonStrings.Null, field);
            return true;
        }

        private static bool TryReadJson(string uri, out string json)
        {
            json = null;
            if (uri == null)
            {
                return false;
            }
            else if (uri.IsUri())
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
            rootedPath = PSRuleOption.GetRootedPath(path);
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
            switch (extension)
            {
                case ".bicep":
                case ".cs":
                case ".csx":
                case ".ts":
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
                case "Jenkinsfile":
                    return "// ";

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
                case ".gitignore":
                case ".pl":
                case ".rb":
                case "Dockerfile":
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
            if (!(fieldValue is PSObject pso))
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
}
