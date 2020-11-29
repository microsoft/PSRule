// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using YamlDotNet.Serialization;

namespace PSRule.Definitions
{
    internal interface IBaselineSpec
    {
        BindingOption Binding { get; set; }

        ConfigurationOption Configuration { get; set; }

        RuleOption Rule { get; set; }
    }

    public sealed class Baseline : Resource<BaselineSpec>, IResource
    {
        public Baseline(SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, BaselineSpec spec)
            : base(metadata)
        {
            Info = info;
            Source = source;
            Spec = spec;
            Name = BaselineId = metadata.Name;
            Obsolete = ResourceHelper.IsObsolete(metadata);
        }

        [YamlIgnore()]
        public readonly string BaselineId;

        [YamlIgnore()]
        public readonly string Name;

        [YamlIgnore()]
        internal readonly bool Obsolete;

        /// <summary>
        /// The script file path where the baseline is defined.
        /// </summary>
        [YamlIgnore()]
        public readonly SourceFile Source;

        public readonly ResourceHelpInfo Info;

        /// <summary>
        /// The name of the module where the baseline is defined, or null if the baseline is not defined in a module.
        /// </summary>
        [YamlIgnore()]
        public readonly string ModuleName;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis;

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

        ResourceKind IResource.Kind => ResourceKind.Baseline;

        string IResource.Id => BaselineId;

        string IResource.Name => Name;

        public override BaselineSpec Spec { get; }
    }

    public sealed class BaselineSpec : Spec, IBaselineSpec
    {
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        public RuleOption Rule { get; set; }
    }

    internal sealed class BaselineFilter : IResourceFilter
    {
        private readonly HashSet<string> _Include;
        private readonly WildcardPattern _WildcardMatch;

        public BaselineFilter(string[] include)
        {
            _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
            _WildcardMatch = null;
            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                _WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        public bool Match(string name, TagSet tag)
        {
            return _Include == null || _Include.Contains(name) || MatchWildcard(ruleName: name);
        }

        private bool MatchWildcard(string ruleName)
        {
            if (_WildcardMatch == null)
                return false;

            return _WildcardMatch.IsMatch(ruleName);
        }
    }

    internal sealed class BaselineRef : ResourceRef
    {
        public readonly OptionContext.ScopeType Type;

        public BaselineRef(string id, OptionContext.ScopeType scopeType)
            : base(id, ResourceKind.Baseline)
        {
            Type = scopeType;
        }
    }
}
