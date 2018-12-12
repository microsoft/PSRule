using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Rules
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleOutcomeReason : byte
    {
        None = 0,

        /// <summary>
        /// The rule was processed.
        /// </summary>
        Processed = 1,

        /// <summary>
        /// The rule was not processed because the precondition returned false.
        /// </summary>
        PreconditionFail = 2,

        /// <summary>
        /// The rule was not processed because a dependency already returned an error or failed.
        /// </summary>
        DependencyFail = 3,

        /// <summary>
        /// The rule was processed but didn't return pass or fail.
        /// </summary>
        Inconclusive = 4
    }
}
