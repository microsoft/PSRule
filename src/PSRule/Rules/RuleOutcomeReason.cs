// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Rules
{
    /// <summary>
    /// The reason for the outcome of a rule.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleOutcomeReason
    {
        /// <summary>
        /// A reason was not specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The rule was processed.
        /// </summary>
        /// <remarks>
        /// This reason is used with the Pass, Fail and Error outcomes.
        /// </remarks>
        Processed = 1,

        /// <summary>
        /// The rule was not processed because the precondition returned false.
        /// </summary>
        /// <remarks>
        /// This reason is used with the None outcome.
        /// </remarks>
        PreconditionFail = 2,

        /// <summary>
        /// The rule was not processed because a dependency already returned an error or failed.
        /// </summary>
        /// <remarks>
        /// This reason is used with the None outcome.
        /// </remarks>
        DependencyFail = 3,

        /// <summary>
        /// The rule was processed but didn't return pass or fail.
        /// </summary>
        /// <remarks>
        /// This reason is used with the Fail outcome.
        /// </remarks>
        Inconclusive = 4,

        /// <summary>
        /// The rule was not processed because the Target Name was suppressed.
        /// </summary>
        /// <remarks>
        /// This reason is used with the None outcome.
        Suppressed = 5
    }
}
