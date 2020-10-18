// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Management.Automation.Language;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// Information about an error that occurred within PSRule.
    /// </summary>
    public sealed class ErrorInfo
    {
        internal ErrorInfo(string message, string scriptStackTrace, string errorId, Exception exception, ErrorCategory category, string positionMessage, IScriptExtent scriptExtent)
        {
            Message = message;
            ScriptStackTrace = scriptStackTrace;
            ErrorId = errorId;
            Exception = exception;
            Category = category;
            ExceptionType = Exception?.GetType()?.FullName;
            PositionMessage = positionMessage;
            ScriptExtent = scriptExtent;
        }

        /// <summary>
        /// An error message describing the issue.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; }

        /// <summary>
        /// A PSRule script stack trace.
        /// </summary>
        [JsonProperty(PropertyName = "scriptStackTrace")]
        public string ScriptStackTrace { get; }

        /// <summary>
        /// A fully qualified identifier of the error.
        /// </summary>
        [JsonProperty(PropertyName = "errorId")]
        public string ErrorId { get; }

        /// <summary>
        /// The related error exception.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public Exception Exception { get; }

        /// <summary>
        /// The exception type.
        /// </summary>
        [JsonProperty(PropertyName = "exceptionType")]
        public string ExceptionType { get; }

        /// <summary>
        /// The error category.
        /// </summary>
        [JsonProperty(PropertyName = "category")]
        public ErrorCategory Category { get; }

        /// <summary>
        /// A positional message for the error.
        /// </summary>
        [JsonIgnore]
        public string PositionMessage { get; }

        /// <summary>
        /// The extent for the error.
        /// </summary>
        [JsonIgnore]
        public IScriptExtent ScriptExtent { get; }
    }
}
