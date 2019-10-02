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
        public AssertResult Create(bool condition, string reason = null)
        {
            if (PipelineContext.CurrentThread.ExecutionScope != ExecutionScope.Condition)
            {
                throw new RuleRuntimeException(string.Format(PSRuleResources.VariableConditionScope, "Assert"));
            }
            return new AssertResult(this, condition, reason);
        }

        public AssertResult Pass()
        {
            return Create(condition: true);
        }

        public AssertResult Fail(string reason = null)
        {
            return Create(condition: false, reason: reason);
        }

        public AssertResult JsonSchema(PSObject inputObject, string uri)
        {
            // Guard parameters
            if (TryNull(inputObject, nameof(inputObject), out AssertResult result) || TryNullOrEmpty(uri, nameof(uri), out result))
            {
                return result;
            }

            // Get the schema
            if (!(TryReadJson(uri, out string schemaContent)))
            {
                return Fail(reason: string.Format(ReasonStrings.JsonSchemaNotFound, uri));
            }

            var s = new JsonSerializer();
            JsonSchemaOptions.OutputFormat = SchemaValidationOutputFormat.Basic;
            var schema = s.Deserialize<JsonSchema>(JsonValue.Parse(schemaContent));

            // Get the TargetObject
            var json = JsonValue.Parse(inputObject.ToJson());

            // Validate
            var schemaResults = schema.Validate(json);

            // Schema is valid
            if (schemaResults.IsValid)
            {
                return Pass();
            }

            // Handle schema invalid
            result = Fail();

            if (!string.IsNullOrEmpty(schemaResults.ErrorMessage))
            {
                result.AddReason(string.Format(ReasonStrings.JsonSchemaInvalid, schemaResults.InstanceLocation.ToString(), schemaResults.ErrorMessage));
            }

            foreach (var r in schemaResults.NestedResults)
            {
                if (!string.IsNullOrEmpty(r.ErrorMessage))
                {
                    result.AddReason(string.Format(ReasonStrings.JsonSchemaInvalid, r.InstanceLocation.ToString(), r.ErrorMessage));
                }
            }
            return result;
        }

        public AssertResult HasField(PSObject inputObject, string field, bool caseSensitive = false)
        {
            // Guard parameters
            if (TryNull(inputObject, nameof(inputObject), out AssertResult result) || TryNullOrEmpty(field, nameof(field), out result))
            {
                return result;
            }

            // Assert
            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: caseSensitive, value: out object fieldValue))
            {
                return Fail(string.Format(ReasonStrings.HasField, field));
            }
            return Pass();
        }

        public AssertResult HasFieldValue(PSObject inputObject, string field, object expectedValue = null)
        {
            // Guard parameters
            if (TryNull(inputObject, nameof(inputObject), out AssertResult result) || TryNullOrEmpty(field, nameof(field), out result))
            {
                return result;
            }

            // Assert
            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue))
            {
                return Fail(string.Format(ReasonStrings.HasField, field));
            }
            else if (IsEmpty(fieldValue))
            {
                return Fail(string.Format(ReasonStrings.HasFieldValue, field));
            }
            else if (expectedValue != null && !IsValue(fieldValue, expectedValue))
            {
                return Fail(string.Format(ReasonStrings.HasExpectedFieldValue, field, fieldValue));
            }
            return Pass();
        }

        /// <summary>
        /// The object should not have the field or the field value is set to the default value.
        /// </summary>
        public AssertResult HasDefaultValue(PSObject inputObject, string field, object defaultValue)
        {
            // Guard parameters
            if (TryNull(inputObject, nameof(inputObject), out AssertResult result) || TryNullOrEmpty(field, nameof(field), out result))
            {
                return result;
            }

            // Assert
            if (!ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue) || IsValue(fieldValue, defaultValue))
            {
                return Pass();
            }
            return Fail(string.Format(ReasonStrings.HasExpectedFieldValue, field, fieldValue));
        }

        public AssertResult NullOrEmpty(PSObject inputObject, string field)
        {
            // Guard parameters
            if (TryNull(inputObject, nameof(inputObject), out AssertResult result) || TryNullOrEmpty(field, nameof(field), out result))
            {
                return result;
            }

            // Assert
            if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: inputObject, name: field, caseSensitive: false, value: out object fieldValue) && !IsEmpty(fieldValue))
            {
                return Fail(string.Format(ReasonStrings.NullOrEmpty, field));
            }
            return Pass();
        }

        private bool IsEmpty(object fieldValue)
        {
            return fieldValue == null ||
                (fieldValue is ICollection cvalue && cvalue.Count == 0) ||
                (fieldValue is string svalue && string.IsNullOrEmpty(svalue));
        }

        private bool IsValue(object fieldValue, object expectedValue)
        {
            // Unwrap as required
            var expectedBase = expectedValue;
            if (expectedValue is PSObject pso)
            {
                expectedBase = pso.BaseObject;
            }

            if (fieldValue is string && expectedBase is string)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(fieldValue, expectedBase);
            }
            else if (expectedBase.Equals(fieldValue))
            {
                return true;
            }

            return false;
        }

        private bool TryNull(object value, string parameterName, out AssertResult result)
        {
            result = value == null ? FailNull(parameterName) : null;
            return result != null;
        }

        private bool TryNullOrEmpty(string value, string parameterName, out AssertResult result)
        {
            result = string.IsNullOrEmpty(value) ? FailNullOrEmpty(parameterName) : null;
            return result != null;
        }

        private AssertResult FailNull(string parameterName)
        {
            return Fail(reason: string.Format(ReasonStrings.NullParameter, parameterName));
        }

        private AssertResult FailNullOrEmpty(string parameterName)
        {
            return Fail(reason: string.Format(ReasonStrings.NullOrEmptyParameter, parameterName));
        }

        private bool TryReadJson(string uri, out string json)
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

        private bool TryFilePath(string uri, out string path)
        {
            path = PSRuleOption.GetRootedPath(uri);
            return File.Exists(path);
        }
    }
}
