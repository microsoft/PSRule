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
using System.Text.RegularExpressions;
using System.Threading;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper variable exposed at runtime for rules.
    /// </summary>
    public sealed class Assert
    {
        private const string COMMASEPARATOR = ", ";
        private const string CACHE_MATCH = "MatchRegex";
        private const string CACHE_MATCH_C = "MatchRegexCaseSensitive";

        public AssertResult Create(bool condition, string reason = null)
        {
            return Create(condition: condition, reason: reason, args: null);
        }

        internal AssertResult Create(bool condition, string reason, object[] args)
        {
            if (!(PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Condition || PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Precondition))
                throw new RuleRuntimeException(string.Format(PSRuleResources.VariableConditionScope, "Assert"));

            return new AssertResult(this, condition, reason, args);
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
                GuardField(inputObject, field, caseSensitive, out _, out result))
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
            else if (expectedValue != null && !IsValue(fieldValue, expectedValue, caseSensitive: false))
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
                || IsValue(fieldValue, defaultValue, caseSensitive: false))
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

            if (!Runtime.SemanticVersion.TryParseConstraint(constraint, out SemanticVersion.Constraint c))
                throw new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VersionConstraintInvalid, value));

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

            if (CompareNumeric(fieldValue, value, out int compare, out object actual))
                return compare > 0 ? Pass() : Fail(ReasonStrings.Greater, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult GreaterOrEqual(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (CompareNumeric(fieldValue, value, out int compare, out object actual))
                return compare >= 0 ? Pass() : Fail(ReasonStrings.GreaterOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult Less(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (CompareNumeric(fieldValue, value, out int compare, out object actual))
                return compare < 0 ? Pass() : Fail(ReasonStrings.Less, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        public AssertResult LessOrEqual(PSObject inputObject, string field, int value)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            if (CompareNumeric(fieldValue, value, out int compare, out object actual))
                return compare <= 0 ? Pass() : Fail(ReasonStrings.LessOrEqual, actual, value);

            return Fail(ReasonStrings.Compare, fieldValue, value);
        }

        /// <summary>
        /// The object field value must be included in the set.
        /// </summary>
        public AssertResult In(PSObject inputObject, string field, Array values, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result))
                return result;

            for (var i = 0; i < values.Length; i++)
            {
                if (AnyValue(fieldValue, values.GetValue(i), caseSensitive, out object foundValue))
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
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: caseSensitive, value: out object fieldValue))
                return Pass();

            for (var i = 0; i < values.Length; i++)
            {
                if (AnyValue(fieldValue, values.GetValue(i), caseSensitive, out object foundValue))
                    return Fail(ReasonStrings.NotIn, foundValue);
            }
            return Pass();
        }

        /// <summary>
        /// The object field value must match the regular expression.
        /// </summary>
        public AssertResult Match(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            var expression = GetRegularExpression(pattern, caseSensitive);
            if (expression.IsMatch(value))
                return Pass();

            return Fail(ReasonStrings.MatchPattern, value, pattern);
        }

        /// <summary>
        /// The object field value must not match the regular expression.
        /// </summary>
        public AssertResult NotMatch(PSObject inputObject, string field, string pattern, bool caseSensitive = false)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result))
                return result;

            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: caseSensitive, value: out object fieldValue))
                return Pass();

            if (GuardString(fieldValue, out string value, out result))
                return result;

            var expression = GetRegularExpression(pattern, caseSensitive);
            if (!expression.IsMatch(value))
                return Pass();

            return Fail(ReasonStrings.NotMatchPattern, value, pattern);
        }

        public AssertResult FilePath(PSObject inputObject, string field, string[] suffix = null)
        {
            // Guard parameters
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            if (suffix == null || suffix.Length == 0)
            {
                if (!TryFilePath(value, out _))
                    return Fail(ReasonStrings.FilePath, value);

                return Pass();
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
            if (GuardNullParam(inputObject, nameof(inputObject), out AssertResult result) ||
                GuardNullOrEmptyParam(field, nameof(field), out result) ||
                GuardField(inputObject, field, false, out object fieldValue, out result) ||
                GuardString(fieldValue, out string value, out result))
                return result;

            // File does not exist
            if (!TryFilePath(value, out _))
                return Fail(ReasonStrings.FilePath, value);

            // No header
            if (header == null || header.Length == 0)
                return Pass();

            if (string.IsNullOrEmpty(prefix))
                prefix = DetectLinePrefix(Path.GetExtension(value));

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
            if (lineNo < header.Length)
                return Fail(ReasonStrings.FileHeader);

            return Pass();
        }

        #region Helper methods

        private static bool IsEmpty(object fieldValue)
        {
            return fieldValue == null ||
                (fieldValue is ICollection cvalue && cvalue.Count == 0) ||
                (fieldValue is string svalue && string.IsNullOrEmpty(svalue));
        }

        /// <summary>
        /// Get the base object.
        /// </summary>
        private static object GetBaseObject(object o)
        {
            return o is PSObject pso ? pso.BaseObject : o;
        }

        private static bool IsValue(object actualValue, object expectedValue, bool caseSensitive)
        {
            var expectedBase = GetBaseObject(expectedValue);
            var actualBase = GetBaseObject(actualValue);
            if (actualBase is string && expectedBase is string)
                return caseSensitive ? StringComparer.Ordinal.Equals(actualBase, expectedBase) : StringComparer.OrdinalIgnoreCase.Equals(actualBase, expectedBase);

            return expectedBase.Equals(actualBase) || expectedValue.Equals(actualValue);
        }

        private static bool AnyValue(object actualValue, object expectedValue, bool caseSensitive, out object foundValue)
        {
            foundValue = actualValue;
            var expectedBase = GetBaseObject(expectedValue);
            if (actualValue is IEnumerable items)
            {
                foreach (var item in items)
                {
                    foundValue = item;
                    if (IsValue(item, expectedBase, caseSensitive))
                        return true;
                }
            }
            if (IsValue(actualValue, expectedBase, caseSensitive))
            {
                foundValue = actualValue;
                return true;
            }
            return false;
        }

        private static bool TryString(object obj, out string value)
        {
            value = null;
            if (GetBaseObject(obj) is string svalue)
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

        private static bool TryLong(object obj, out long value)
        {
            if (obj is long lvalue)
            {
                value = lvalue;
                return true;
            }
            value = 0;
            return false;
        }

        private static bool TryFloat(object obj, out float value)
        {
            if (obj is float fvalue)
            {
                value = fvalue;
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

        private static bool CompareNumeric(object obj, int value, out int compare, out object actual)
        {
            if (TryInt(obj, out int iactual) || TryStringLength(obj, out iactual) || TryArrayLength(obj, out iactual))
            {
                compare = iactual.CompareTo(value);
                actual = iactual;
                return true;
            }
            if (TryLong(obj, out long lactual))
            {
                compare = lactual.CompareTo(value);
                actual = lactual;
                return true;
            }
            if (TryFloat(obj, out float factual))
            {
                compare = factual.CompareTo(value);
                actual = factual;
                return true;
            }
            compare = 0;
            actual = 0;
            return false;
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
        /// <returns>Returns true if the field does not exist.</returns>
        /// <remarks>
        /// Reason: The parameter '{0}' is null or empty.
        /// </remarks>
        private bool GuardNullOrEmptyParam(string value, string parameterName, out AssertResult result)
        {
            result = string.IsNullOrEmpty(value) ? Fail(ReasonStrings.NullOrEmptyParameter, parameterName) : null;
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

        /// <summary>
        /// Fails if the field value is not a string.
        /// </summary>
        /// <returns>Return true if the field value is not a string.</returns>
        /// <remarks>
        /// Reason: The field value '{0}' is not a string.
        /// </remarks>
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

        private static bool TryFilePath(string path, out string rootedPath)
        {
            rootedPath = PSRuleOption.GetRootedPath(path);
            return File.Exists(rootedPath);
        }

        private static string FormatArray(string[] values)
        {
            return string.Join(COMMASEPARATOR, values);
        }

        private static Regex GetRegularExpression(string pattern, bool caseSensitive)
        {
            if (!TryPipelineCache(caseSensitive ? CACHE_MATCH_C : CACHE_MATCH, pattern, out Regex expression))
            {
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                expression = new Regex(pattern, options);
                SetPipelineCache(CACHE_MATCH, pattern, expression);
            }
            return expression;
        }

        /// <summary>
        /// Try to retrieve the cached key from the pipeline cache.
        /// </summary>
        private static bool TryPipelineCache<T>(string prefix, string key, out T value)
        {
            value = default(T);
            if (PipelineContext.CurrentThread.ExpressionCache.TryGetValue(string.Concat(prefix, key), out object ovalue))
            {
                value = (T)ovalue;
                return true;
            }
            return false;
        }

        private static void SetPipelineCache<T>(string prefix, string key, T value)
        {
            PipelineContext.CurrentThread.ExpressionCache[string.Concat(prefix, key)] = value;
        }

        /// <summary>
        /// Determine line comment prefix by file extension
        /// </summary>
        private static string DetectLinePrefix(string extension)
        {
            switch (extension)
            {
                case ".cs":
                case ".ts":
                case ".js":
                case ".fs":
                case ".go":
                case ".php":
                case ".cpp":
                case ".h":
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
                    return "# ";

                case ".sql":
                case ".lua":
                    return "-- ";

                default:
                    return string.Empty;
            }
        }

        #endregion Helper methods
    }
}
