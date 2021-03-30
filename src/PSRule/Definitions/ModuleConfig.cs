// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions
{
    /// <summary>
    /// A module configuration resource.
    /// </summary>
    [Spec(Specs.V1, Specs.ModuleConfig)]
    internal sealed class ModuleConfig : InternalResource<ModuleConfigSpec>
    {
        public ModuleConfig(string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, ModuleConfigSpec spec)
            : base(ResourceKind.ModuleConfig, apiVersion, source, metadata, info, spec) { }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the module config.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis;
    }

    /// <summary>
    /// A specification for a module configuration.
    /// </summary>
    public sealed class ModuleConfigSpec : Spec
    {
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        public ConventionOption Convention { get; set; }

        public OutputOption Output { get; set; }
    }
}
