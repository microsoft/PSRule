using Newtonsoft.Json;

namespace PSRule.Rules
{
    /// <summary>
    /// Define a single rule.
    /// </summary>
    [JsonObject]
    public sealed class Rule
    {
        [JsonProperty(PropertyName = "sourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// One or more tags assigned to the rule. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        [JsonProperty(PropertyName = "tag")]
        public TagSet Tag { get; set; }
    }
}
