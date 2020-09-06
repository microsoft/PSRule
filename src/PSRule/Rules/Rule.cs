// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Host;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// Define a single rule.
    /// </summary>
    [JsonObject]
    public sealed class Rule : IDependencyTarget, ITargetInfo
    {
        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        [JsonProperty(PropertyName = "ruleId", Required = Required.Always)]
        public string RuleId { get; set; }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "ruleName", Required = Required.Always)]
        public string RuleName { get; set; }

        /// <summary>
        /// The script file path where the rule is defined.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string SourcePath => Source.Path;

        /// <summary>
        /// The name of the module where the rule is defined, or null if the rule is not defined in a module.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string ModuleName => Source.ModuleName;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis;

        // Alias to synopsis
        [JsonIgnore]
        [YamlIgnore]
        public string Description => Info.Synopsis;

        /// <summary>
        /// One or more tags assigned to the rule. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        [JsonProperty(PropertyName = "tag")]
        [DefaultValue(null)]
        public TagSet Tag { get; set; }

        [JsonProperty(PropertyName = "info")]
        [DefaultValue(null)]
        public RuleHelpInfo Info { get; set; }

        [JsonProperty(PropertyName = "source")]
        [DefaultValue(null)]
        public SourceFile Source { get; set; }

        /// <summary>
        /// Other rules that must completed successfully before calling this rule.
        /// </summary>
        [JsonProperty(PropertyName = "dependsOn")]
        public string[] DependsOn { get; set; }

        string ITargetInfo.TargetName => RuleName;

        string ITargetInfo.TargetType => typeof(Rule).FullName;
    }
}
