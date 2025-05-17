// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Rules;

/// <summary>
/// A rule resource V1.
/// </summary>
[Spec(Specs.V1, Specs.Rule)]
internal sealed class RuleV1 : InternalResource<RuleV1Spec>, IResource, IRuleV1
{
    // internal const SeverityLevel DEFAULT_LEVEL = SeverityLevel.Error;

    public RuleV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, RuleV1Spec spec)
        : base(ResourceKind.Rule, apiVersion, source, metadata, info, extent, spec)
    {
        Ref = ResourceHelper.GetIdNullable(source.Module, metadata.Ref, ResourceIdKind.Ref);
        Alias = ResourceHelper.GetResourceId(source.Module, metadata.Alias, ResourceIdKind.Alias);
        Level = ResourceHelper.GetLevel(spec.Level);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    [YamlIgnore]
    public ResourceId? Ref { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    [YamlIgnore]
    public ResourceId[] Alias { get; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public SeverityLevel Level { get; }

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Synopsis => Info.Synopsis.Text;

    /// <inheritdoc/>
    ResourceId? IDependencyTarget.Ref => Ref;

    /// <inheritdoc/>
    ResourceId[] IDependencyTarget.Alias => Alias;

    // Not supported with resource rules.
    ResourceId[] IDependencyTarget.DependsOn => Array.Empty<ResourceId>();

    /// <inheritdoc/>
    bool IDependencyTarget.Dependency => Source.IsDependency();

    /// <inheritdoc/>
    ResourceId? IResource.Ref => Ref;

    /// <inheritdoc/>
    ResourceId[] IResource.Alias => Alias;

    /// <inheritdoc/>
    IResourceTags IRuleV1.Tag => Metadata.Tags;

    /// <inheritdoc/>
    InfoString IRuleV1.Recommendation => InfoString.Create(Spec?.Recommend);
}
