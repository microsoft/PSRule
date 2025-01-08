// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A module configuration resource.
/// </summary>
[Spec(Specs.V1, Specs.ModuleConfig)]
internal sealed class ModuleConfigV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, ModuleConfigV1Spec spec)
    : InternalResource<ModuleConfigV1Spec>(ResourceKind.ModuleConfig, apiVersion, source, metadata, info, extent, spec), IModuleConfig
{

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the module config.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Synopsis => Info.Synopsis.Text;

    IModuleConfigSpec IResource<IModuleConfigSpec>.Spec => Spec;
}
