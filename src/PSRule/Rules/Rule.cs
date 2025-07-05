// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using YamlDotNet.Serialization;

namespace PSRule.Rules;

#pragma warning disable CS8618 // Keyword required is not supported in .NET Standard 2.0

/// <summary>
/// Define a single rule.
/// </summary>
[JsonObject]
public sealed class Rule : IDependencyTarget, ITargetInfo, IResource, IRuleV1
{
    /// <summary>
    /// A unique identifier for the rule.
    /// </summary>
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public ResourceId Id { get; set; }

    /// <summary>
    /// The name of the rule.
    /// </summary>
    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public string Name => Id.Name;

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [JsonProperty(PropertyName = "level")]
    public SeverityLevel Level { get; set; }

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the rule.
    /// </summary>
    [JsonIgnore, YamlIgnore]
    public string Synopsis => Info.Synopsis;

    /// <summary>
    /// One or more tags assigned to the rule. Tags are additional metadata used to select rules to execute and identify results.
    /// </summary>
    [JsonProperty(PropertyName = "tag")]
    [DefaultValue(null)]
    public IResourceTags Tag { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "info")]
    [DefaultValue(null)]
    public RuleHelpInfo Info { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "source")]
    [DefaultValue(null)]
    public ISourceFile Source { get; set; }

    /// <summary>
    /// Other rules that must completed successfully before calling this rule.
    /// </summary>
    [JsonProperty(PropertyName = "dependsOn")]
    public ResourceId[] DependsOn { get; set; }

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    public ResourceFlags Flags { get; set; }

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    public ISourceExtent Extent { get; set; }

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    public IResourceLabels Labels { get; set; }

    string ITargetInfo.TargetName => Name;

    string ITargetInfo.TargetType => typeof(Rule).FullName;

    TargetSourceInfo ITargetInfo.Source => new() { File = Source.Path };

    bool IDependencyTarget.Dependency => Source.IsDependency();

    ResourceKind IResource.Kind => ResourceKind.Rule;

    string IResource.ApiVersion => Specs.V1;

    string IResource.Name => Name;

    IResourceHelpInfo IResource.Info => Info;

    IResourceTags IResource.Tags => Tag;

    InfoString IRuleV1.Recommendation => ((IRuleHelpInfoV2)Info)?.Recommendation;

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    public ResourceId? Ref { get; set; }

    /// <inheritdoc/>
    [JsonIgnore, YamlIgnore]
    public ResourceId[] Alias { get; set; }
}

#pragma warning restore CS8618
