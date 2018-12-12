using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PSRule.Rules
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleOutcome : byte
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

        Processed = Fail | Pass | Error,
        All = 255
    }
}
