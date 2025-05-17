// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Text;
using Newtonsoft.Json;
using PSRule.Definitions;
using YamlDotNet.Serialization;

namespace PSRule.Rules;

/// <summary>
/// Output view helper class for rule help.
/// </summary>
public sealed class RuleHelpInfo : IRuleHelpInfoV2
{
    private readonly InfoString _Synopsis;
    private readonly InfoString _Description;
    private readonly InfoString _Recommendation;

    internal RuleHelpInfo(string name, string displayName, string moduleName, InfoString? synopsis = null, InfoString? description = null, InfoString? recommendation = null)
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

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    InfoString IRuleHelpInfoV2.Recommendation => _Recommendation;

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    InfoString IResourceHelpInfo.Synopsis => _Synopsis;

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    InfoString IResourceHelpInfo.Description => _Description;

    /// <summary>
    /// Get a view link string for display in rule help.
    /// </summary>
    public string? GetLinkString()
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
            sb.Append(System.Environment.NewLine);
        }
        return sb.ToString();
    }
}
