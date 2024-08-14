// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Baselines;

/// <summary>
/// A baseline resource V1.
/// </summary>
[Spec(Specs.V1, Specs.Baseline)]
public sealed class Baseline : InternalResource<BaselineSpec>, IResource
{
    /// <summary>
    /// Create a baseline instance.
    /// </summary>
    public Baseline(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, BaselineSpec spec)
        : base(ResourceKind.Baseline, apiVersion, source, metadata, info, extent, spec) { }

    /// <summary>
    /// The unique identifier for the baseline.
    /// </summary>
    [YamlIgnore()]
    public string BaselineId => Name;

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Synopsis => Info.Synopsis.Text;
}
