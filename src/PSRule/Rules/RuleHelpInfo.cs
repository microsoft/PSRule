// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using PSRule.Definitions;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// A rule help information structure.
    /// </summary>
    public interface IRuleHelpInfoV2 : IResourceHelpInfo
    {
        /// <summary>
        /// The rule recommendation.
        /// </summary>
        InfoString Recommendation { get; }

        /// <summary>
        /// Additional annotations, which are string key/ value pairs.
        /// </summary>
        Hashtable Annotations { get; }

        /// <summary>
        /// The name of the module where the rule was loaded from.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Additional online links to reference information for the rule.
        /// </summary>
        Link[] Links { get; }
    }

    /// <summary>
    /// Extension methods for rule help information.
    /// </summary>
    public static class RuleHelpInfoExtensions
    {
        private const string ONLINE_HELP_LINK_ANNOTATION = "online version";

        /// <summary>
        /// Get the URI for the online version of the documentation.
        /// </summary>
        /// <returns>Returns the URI when a valid link is set, otherwise null is returned.</returns>
        public static Uri GetOnlineHelpUri(this IRuleHelpInfoV2 info)
        {
            var link = GetOnlineHelpUrl(info);
            return link == null ||
                !Uri.TryCreate(link, UriKind.Absolute, out var result) ?
                null : result;
        }

        /// <summary>
        /// Get the URL for the online version of the documentation.
        /// </summary>
        /// <returns>Returns the URL when set, otherwise null is returned.</returns>
        public static string GetOnlineHelpUrl(this IRuleHelpInfoV2 info)
        {
            return info == null ||
                info.Annotations == null ||
                !info.Annotations.ContainsKey(ONLINE_HELP_LINK_ANNOTATION) ?
                null : info.Annotations[ONLINE_HELP_LINK_ANNOTATION].ToString();
        }
    }

    /// <summary>
    /// An URL link to reference information.
    /// </summary>
    public sealed class Link
    {
        internal Link(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }

        /// <summary>
        /// The display name of the link.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The URL to the information, or the target link.
        /// </summary>
        public string Uri { get; }
    }

    /// <summary>
    /// Output view helper class for rule help.
    /// </summary>
    public sealed class RuleHelpInfo : IRuleHelpInfoV2
    {
        private readonly InfoString _Synopsis;
        private readonly InfoString _Description;
        private readonly InfoString _Recommendation;

        internal RuleHelpInfo(string name, string displayName, string moduleName, InfoString synopsis = null, InfoString description = null, InfoString recommendation = null)
        {
            Name = name;
            DisplayName = displayName;
            ModuleName = moduleName;
            _Synopsis = synopsis ?? new InfoString();
            _Description = description ?? new InfoString();
            _Recommendation = recommendation ?? new InfoString();
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
        public string Synopsis => _Synopsis.Text;

        /// <summary>
        /// An extended description of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description => _Description.Text;

        /// <summary>
        /// The recommendation for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "recommendation")]
        public string Recommendation => _Recommendation.Text;

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

        [JsonIgnore, YamlIgnore]
        InfoString IRuleHelpInfoV2.Recommendation => _Recommendation;

        [JsonIgnore, YamlIgnore]
        InfoString IResourceHelpInfo.Synopsis => _Synopsis;

        [JsonIgnore, YamlIgnore]
        InfoString IResourceHelpInfo.Description => _Description;

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
