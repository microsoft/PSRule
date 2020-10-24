// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The PowerShell informational stream to log specific outcomes to.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutcomeLogStream
    {
        /// <summary>
        /// Outcomes will not be logged to an informational stream.
        /// </summary>
        None = 0,

        /// <summary>
        /// Log to Error stream.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Log to Warning stream.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Log to Information stream.
        /// </summary>
        Information = 3
    }
}
