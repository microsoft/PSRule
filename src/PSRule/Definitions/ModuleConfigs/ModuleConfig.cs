// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.ModuleConfigs
{
    /// <summary>
    /// A module configuration resource.
    /// </summary>
    [Spec(Specs.V1, Specs.ModuleConfig)]
    internal sealed class ModuleConfigV1 : InternalResource<ModuleConfigV1Spec>
    {
        public ModuleConfigV1(string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, ModuleConfigV1Spec spec)
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
    internal sealed class ModuleConfigV1Spec : Spec
    {
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        public ConventionOption Convention { get; set; }

        public OutputOption Output { get; set; }

        public RuleOption Rule { get; set; }
    }
}
