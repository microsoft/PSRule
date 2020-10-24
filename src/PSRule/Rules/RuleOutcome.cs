// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Rules
{
    /// <summary>
    /// The outcome of a rule.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleOutcome
    {
        /// <summary>
        /// The rule was not evaluated.
        /// </summary>
        None = 0,

        /// <summary>
        /// The rule evaluated as false.
        /// </summary>
        Fail = 1,

        /// <summary>
        /// The rule evaluated as true.
        /// </summary>
        Pass = 2,

        /// <summary>
        /// The rule returned an error.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Any outcome when the rule was processed.
        /// </summary>
        /// <remarks>
        /// This flag is used to filter outcomes with Invoke-PSRule.
        /// </remarks>
        Processed = Pass | Fail | Error,

        /// <summary>
        /// Any outcome.
        /// </summary>
        /// <remarks>
        /// This flag is used to filter outcomes with Invoke-PSRule.
        /// </remarks>
        All = 255
    }
}
