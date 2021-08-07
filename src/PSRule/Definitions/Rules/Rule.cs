// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions.Expressions;
using PSRule.Host;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Rules
{
    public interface IRule
    {
    }

    internal interface IRuleSpec
    {
        LanguageIf Condition { get; }

        string[] Type { get; }

        string[] With { get; }
    }

    [Spec(Specs.V1, Specs.Rule)]
    internal sealed class RuleV1 : InternalResource<RuleV1Spec>, IResource
    {
        public RuleV1(string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, RuleV1Spec spec)
            : base(ResourceKind.Rule, apiVersion, source, metadata, info, spec) { }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis;

        string ILanguageBlock.Id => Name;
    }


    internal sealed class RuleV1Spec : Spec, IRuleSpec
    {
        public LanguageIf Condition { get; set; }

        /// <summary>
        /// An optional type precondition before the rule is evaluated.
        /// </summary>
        public string[] Type { get; set; }

        /// <summary>
        /// An optional selector precondition before the rule is evaluated.
        /// </summary>
        public string[] With { get; set; }
    }
}
