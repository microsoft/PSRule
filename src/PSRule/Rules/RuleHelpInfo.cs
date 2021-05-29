// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// Output view helper class for rule help.
    /// </summary>
    public sealed class RuleHelpInfo
    {
        private const string ONLINE_HELP_LINK_ANNOTATION = "online version";

        internal RuleHelpInfo(string name, string displayName, string moduleName)
        {
            Name = name;
            DisplayName = displayName;
            ModuleName = moduleName;
        }

        public sealed class Link
        {
            internal Link(string name, string uri)
            {
                Name = name;
                Uri = uri;
            }

            public string Name { get; private set; }

            public string Uri { get; private set; }
        }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        /// <summary>
        /// A localized display name for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// The name of the module.
        /// </summary>
        /// <remarks>
        /// This will be null if the rule is not contained within a module.
        /// </remarks>
        [JsonProperty(PropertyName = "moduleName")]
        public string ModuleName { get; private set; }

        /// <summary>
        /// The synopsis of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "synopsis")]
        public string Synopsis { get; internal set; }

        /// <summary>
        /// An extended description of the rule.
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
        [JsonIgnore, YamlIgnore]
        public string Notes { get; internal set; }

        /// <summary>
        /// Reference links for the rule.
        /// </summary>
        [JsonIgnore, YamlIgnore]
        public Link[] Links { get; internal set; }

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
                return null;

            if (Uri.TryCreate(Annotations[ONLINE_HELP_LINK_ANNOTATION].ToString(), UriKind.Absolute, out Uri result))
                return result;

            return null;
        }

        /// <summary>
        /// Get a view link string for display in rule help.
        /// </summary>
        public string GetLinkString()
        {
            if (Links == null)
                return null;

            var sb = new StringBuilder();
            for (var i = 0; i < Links.Length; i++)
            {
                sb.Append(Links[i].Name);
                if (!string.IsNullOrEmpty(Links[i].Uri))
                {
                    sb.Append(": ");
                    sb.Append(Links[i].Uri);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
