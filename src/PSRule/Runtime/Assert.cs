// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Manatee.Json;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Net;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper variable exposed at runtime for rules.
    /// </summary>
    public sealed class Assert
    {
        private const string COMMASEPARATOR = ", ";

        public AssertResult Create(bool condition, string reason = null)
        {
            if (!(PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Condition || PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Precondition))
                throw new RuleRuntimeException(string.Format(PSRuleResources.VariableConditionScope, "Assert"));

            return new AssertResult(this, condition, reason);
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
        /// <param name="reason">An optional reason why the assertion failed.</param>
        public AssertResult Fail(string reason = null)
        {
            return Create(condition: false, reason: reason);
        }

        /// <summary>
        /// Fail the assertion.
        /// </summary>
        /// <param name="reason">An unformatted reason why the assertion failed.</param>
        /// <param name="args">Additional parameters for the reason format.</param>
        private AssertResult Fail(string reason, params object[] args)
        {
            return Create(condition: false, reason: string.Format(reason, args));
        }

        /// <summary>
        /// The object should match the defined schema.
        /// </summary>
        public AssertResult JsonSchema(PSObject inputObject, string uri)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(uri, nameof(uri), out result))
                return result;

            // Get the schema
            if (!(TryReadJson(uri, out string schemaContent)))
                return Fail(ReasonStrings.JsonSchemaNotFound, uri);

            var s = new JsonSerializer();
            JsonSchemaOptions.OutputFormat = SchemaValidationOutputFormat.Basic;
            var schema = s.Deserialize<JsonSchema>(JsonValue.Parse(schemaContent));

            // Get the TargetObject
            var json = JsonValue.Parse(inputObject.ToJson());

            // Validate
            var schemaResults = schema.Validate(json);

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
        public AssertResult HasJsonSchema(PSObject inputObject, string[] uri = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardField(inputObject, "$schema", false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            if (uri == null || uri.Length == 0)
                return Pass();

            for (var i = 0; i < uri.Length; i++)
                if (StringComparer.OrdinalIgnoreCase.Equals(value, uri[i]))
                    return Pass();

            return Fail(ReasonStrings.HasJsonSchema, value);
        }

        /// <summary>
        /// The object should have a specific field.
        /// </summary>
        public AssertResult HasField(PSObject inputObject, string field, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, caseSensitive, out object fieldValue, out result))
                return result;

            // Assert
            return Pass();
        }

        /// <summary>
        /// The object should have a specific field with a value set.
        /// </summary>
        public AssertResult HasFieldValue(PSObject inputObject, string field, object expectedValue = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            // Assert
            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue))
                return Fail(ReasonStrings.HasField, field);
            else if (IsEmpty(fieldValue))
                return Fail(ReasonStrings.HasFieldValue, field);
            else if (expectedValue != null && !IsValue(fieldValue, expectedValue))
                return Fail(ReasonStrings.HasExpectedFieldValue, field, fieldValue);

            return Pass();
        }

        /// <summary>
        /// The object should not have the field or the field value is set to the default value.
        /// </summary>
        public AssertResult HasDefaultValue(PSObject inputObject, string field, object defaultValue)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            // Assert
            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue)
                || IsValue(fieldValue, defaultValue))
                return Pass();

            return Fail(ReasonStrings.HasExpectedFieldValue, field, fieldValue);
        }

        /// <summary>
        /// The object should not have the field or the field value is null or empty.
        /// </summary>
        public AssertResult NullOrEmpty(PSObject inputObject, string field)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            // Assert
            if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue) && !IsEmpty(fieldValue))
                return Fail(ReasonStrings.NullOrEmpty, field);

            return Pass();
        }

        /// <summary>
        /// The object field value should start with the any of the specified prefixes. Only applies to strings.
        /// </summary>
        public AssertResult StartsWith(PSObject inputObject, string field, string[] prefix, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            // Assert
            for (var i = 0; i < prefix.Length; i++)
                if (value.StartsWith(prefix[i], caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                    return Pass();
            
            return Fail(ReasonStrings.StartsWith, field, FormatArray(prefix));
        }

        /// <summary>
        /// The object field value should end with the any of the specified suffix. Only applies to strings.
        /// </summary>
        public AssertResult EndsWith(PSObject inputObject, string field, string[] suffix, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            // Assert
            for (var i = 0; i < suffix.Length; i++)
                if (value.EndsWith(suffix[i], caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                    return Pass();

            return Fail(ReasonStrings.EndsWith, field, FormatArray(suffix));
        }

        /// <summary>
        /// The object field value should contain with the any of the specified text. Only applies to strings.
        /// </summary>
        public AssertResult Contains(PSObject inputObject, string field, string[] text, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            // Assert
            for (var i = 0; i < text.Length; i++)
                if (value.IndexOf(text[i], caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0)
                    return Pass();

            return Fail(ReasonStrings.Contains, field, FormatArray(text));
        }

        /// <summary>
        /// The object field value should match the version constraint. Only applies to strings.
        /// </summary>
        public AssertResult Version(PSObject inputObject, string field, string constraint = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardSemanticVersion(fieldValue, out SemanticVersion.Version value, out result))
                return result;

            Runtime.SemanticVersion.TryParseConstraint(constraint, out SemanticVersion.Constraint c);

            // Assert
            if (c != null && !c.Equals(value))
                return Fail(ReasonStrings.VersionContraint, value, constraint);

            return Pass();
        }

        public AssertResult Greater(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (TryInt(fieldValue, out int actual) || TryStringLength(fieldValue, out actual) || TryArrayLength(fieldValue, out actual))
                return actual > value ? Pass() : Fail(ReasonStrings.Greater, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult GreaterOrEqual(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (TryInt(fieldValue, out int actual) || TryStringLength(fieldValue, out actual) || TryArrayLength(fieldValue, out actual))
                return actual >= value ? Pass() : Fail(ReasonStrings.GreaterOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult Less(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (TryInt(fieldValue, out int actual) || TryStringLength(fieldValue, out actual) || TryArrayLength(fieldValue, out actual))
                return actual < value ? Pass() : Fail(ReasonStrings.Less, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult LessOrEqual(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (TryInt(fieldValue, out int actual) || TryStringLength(fieldValue, out actual) || TryArrayLength(fieldValue, out actual))
                return actual <= value ? Pass() : Fail(ReasonStrings.LessOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        #region Helper methods

        private static bool IsEmpty(object fieldValue)
        {
            return fieldValue == null ||
                (fieldValue is ICollection cvalue && cvalue.Count == 0) ||
                (fieldValue is string svalue && string.IsNullOrEmpty(svalue));
        }

        private static bool IsValue(object fieldValue, object expectedValue)
        {
            // Unwrap as required
            var expectedBase = expectedValue;
            if (expectedValue is PSObject pso)
                expectedBase = pso.BaseObject;

            if (fieldValue is string && expectedBase is string)
                return StringComparer.OrdinalIgnoreCase.Equals(fieldValue, expectedBase);

            return expectedBase.Equals(fieldValue);
        }

        private static bool TryString(object obj, out string value)
        {
            value = null;
            if (obj is string svalue)
            {
                value = svalue;
                return true;
            }
            return false;
        }

        private static bool TryInt(object obj, out int value)
        {
            if (obj is int ivalue)
            {
                value = ivalue;
                return true;
            }
            value = 0;
            return false;
        }

        private static bool TryStringLength(object obj, out int value)
        {
            if (obj is string s)
            {
                value = s.Length;
                return true;
            }
            value = 0;
            return false;
        }

        private static bool TryArrayLength(object obj, out int value)
        {
            if (obj is Array array)
            {
                value = array.Length;
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Fails if the value is null.
        /// </summary>
        private bool GuardNullParam(object value, string parameterName, out AssertResult result)
        {
            result = value == null ? Fail(ReasonStrings.NullParameter, parameterName) : null;
            return result != null;
        }

        /// <summary>
        /// Fails of the value is null or empty.
        /// </summary>
        /// <returns>Returns true if the field does not exist.</returns>
        private bool GuardNullOrEmptyParam(string value, string parameterName, out AssertResult result)
        {
            result = string.IsNullOrEmpty(value) ? Fail(ReasonStrings.NullOrEmptyParameter, parameterName) : null;
            return result != null;
        }

        /// <summary>
        /// Fails if the field does not exist.
        /// </summary>
        /// <returns>Returns true if the field does not exist.</returns>
        private bool GuardField(PSObject inputObject, string field, bool caseSensitive, out object fieldValue, out AssertResult result)
        {
            result = null;
            if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: caseSensitive, value: out fieldValue))
                return false;

            result = Fail(ReasonStrings.HasField, field);
            return true;
        }

        private bool GuardSemanticVersion(object fieldValue, out SemanticVersion.Version value, out AssertResult result)
        {
            result = null;
            value = null;
            if (TryString(fieldValue, out string sversion) && Runtime.SemanticVersion.TryParseVersion(sversion, out value))
                return false;

            result = Fail(ReasonStrings.Version, fieldValue);
            return true;
        }

        private bool GuardString(object fieldValue, out string value, out AssertResult result)
        {
            result = null;
            if (TryString(fieldValue, out value))
                return false;

            result = Fail(ReasonStrings.String, fieldValue);
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
                using (var webClient = new WebClient())
                {
                    json = webClient.DownloadString(uri);
                    return true;
                }
            }
            else if (TryFilePath(uri, out string path))
            {
                using (var reader = new StreamReader(path))
                {
                    json = reader.ReadToEnd();
                    return true;
                }
            }
            return false;
        }

        private static bool TryFilePath(string uri, out string path)
        {
            path = PSRuleOption.GetRootedPath(uri);
            return File.Exists(path);
        }

        private static string FormatArray(string[] values)
        {
            return string.Join(COMMASEPARATOR, values);
        }

        #endregion Helper methods
    }
}
