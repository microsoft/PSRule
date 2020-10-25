// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions
{
    /// <summary>
    /// A module configuration resource.
    /// </summary>
    internal sealed class ModuleConfig : Resource<ModuleConfigSpec>, IResource
    {
        public ModuleConfig(SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, ModuleConfigSpec spec)
            : base(metadata)
        {
            Info = info;
            Source = source;
            Spec = spec;
            Name = metadata.Name;
        }

        [YamlIgnore()]
        public readonly string Name;

        /// <summary>
        /// The path where the module configuration is defined.
        /// </summary>
        [YamlIgnore()]
        public readonly SourceFile Source;

        public readonly ResourceHelpInfo Info;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the module config.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis;

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

        ResourceKind IResource.Kind => ResourceKind.ModuleConfig;

        string IResource.Id => Name;

        string IResource.Name => Name;

        public override ModuleConfigSpec Spec { get; }
    }

    /// <summary>
    /// A specification for a module configuration.
    /// </summary>
    public sealed class ModuleConfigSpec : Spec
    {
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        public OutputOption Output { get; set; }
    }
}
