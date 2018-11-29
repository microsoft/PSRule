using Newtonsoft.Json;
using System.Management.Automation;

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

        [JsonIgnore]
        public ScriptBlock Body { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public TagSet Tag { get; set; }
    }
}
