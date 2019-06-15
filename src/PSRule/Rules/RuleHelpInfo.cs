using Newtonsoft.Json;
using System;
using System.Collections;

namespace PSRule.Rules
{
    /// <summary>
    /// Output view helper class for rule help.
    /// </summary>
    public sealed class RuleHelpInfo
    {
        private const string ONLINE_HELP_LINK_ANNOTATION = "online version";

        internal RuleHelpInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        /// <summary>
        /// The synopsis of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "synopsis")]
        public string Synopsis { get; internal set; }

        /// <summary>
        /// An extented description of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; internal set; }

        /// <summary>
        /// The recommendation for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "recommendation")]
        public string Recommendation { get; internal set; }

        /// <summary>
        /// Additional notes for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; internal set; }

        /// <summary>
        /// Metadata annotations for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "annotations")]
        public Hashtable Annotations { get; internal set; }

        /// <summary>
        /// Get the URI for the online version of the documentation.
        /// </summary>
        /// <returns>A URI when a valid link is set. Otherwise null is returned.</returns>
        public Uri GetOnlineHelpUri()
        {
            if (Annotations == null || !Annotations.ContainsKey(ONLINE_HELP_LINK_ANNOTATION))
            {
                return null;
            }

            if (Uri.TryCreate(Annotations[ONLINE_HELP_LINK_ANNOTATION].ToString(), UriKind.Absolute, out Uri result))
            {
                return result;
            }

            return null;
        }
    }
}
