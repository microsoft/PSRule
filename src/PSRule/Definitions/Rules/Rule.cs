// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Rules
{
    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    public enum SeverityLevel
    {
        /// <summary>
        /// Severity is unset.
        /// </summary>
        None = 0,

        /// <summary>
        /// A failure generates an error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// A failure generates a warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// A failure generate an informational message.
        /// </summary>
        Information = 3
    }

    /// <summary>
    /// A rule resource V1.
    /// </summary>
    public interface IRuleV1 : IResource, IDependencyTarget
    {
        /// <summary>
        /// Obsolete. The name of the rule.
        /// Replaced by <see cref="IResource.Name"/>.
        /// </summary>
        [Obsolete("Use Name instead.")]
        string RuleName { get; }

        /// <summary>
        /// If the rule fails, how serious is the result.
        /// </summary>
        SeverityLevel Level { get; }

        /// <summary>
        /// A recommendation for the rule.
        /// </summary>
        InfoString Recommendation { get; }

        /// <summary>
        /// A short description of the rule.
        /// </summary>
        string Synopsis { get; }

        /// <summary>
        /// Obsolete. A short description of the rule.
        /// Replaced by <see cref="Synopsis"/>.
        /// </summary>
        [Obsolete("Use Synopsis instead.")]
        string Description { get; }

        /// <summary>
        /// Any additional tags assigned to the rule.
        /// </summary>
        ResourceTags Tag { get; }
    }

    /// <summary>
    /// A specification for a rule resource.
    /// </summary>
    internal interface IRuleSpec
    {
        /// <summary>
        /// The of the rule condition that will be evaluated.
        /// </summary>
        LanguageIf Condition { get; }

        /// <summary>
        /// If the rule fails, how serious is the result.
        /// </summary>
        SeverityLevel? Level { get; }

        /// <summary>
        /// An optional type pre-condition before the rule is evaluated.
        /// </summary>
        string[] Type { get; }

        /// <summary>
        /// An optional selector pre-condition before the rule is evaluated.
        /// </summary>
        string[] With { get; }

        /// <summary>
        /// An optional sub-selector pre-condition before the rule is evaluated.
        /// </summary>
        LanguageIf Where { get; }
    }

    /// <summary>
    /// A rule resource V1.
    /// </summary>
    [Spec(Specs.V1, Specs.Rule)]
    internal sealed class RuleV1 : InternalResource<RuleV1Spec>, IResource, IRuleV1
    {
        internal const SeverityLevel DEFAULT_LEVEL = SeverityLevel.Error;

        public RuleV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, RuleV1Spec spec)
            : base(ResourceKind.Rule, apiVersion, source, metadata, info, extent, spec)
        {
            Ref = ResourceHelper.GetIdNullable(source.Module, metadata.Ref, ResourceIdKind.Ref);
            Alias = ResourceHelper.GetRuleId(source.Module, metadata.Alias, ResourceIdKind.Alias);
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
        string IRuleV1.RuleName => Name;

        /// <inheritdoc/>
        ResourceTags IRuleV1.Tag => Metadata.Tags;

        /// <inheritdoc/>
        string IRuleV1.Description => Info.Synopsis.Text;

        /// <inheritdoc/>
        InfoString IRuleV1.Recommendation => InfoString.Create(Spec?.Recommend);
    }

    /// <summary>
    /// A specification for a V1 rule resource.
    /// </summary>
    internal sealed class RuleV1Spec : Spec, IRuleSpec
    {
        /// <inheritdoc/>
        public LanguageIf Condition { get; set; }

        /// <inheritdoc/>
        public SeverityLevel? Level { get; set; }

        /// <inheritdoc/>
        public string Recommend { get; set; }

        /// <inheritdoc/>
        public string[] Type { get; set; }

        /// <inheritdoc/>
        public string[] With { get; set; }

        /// <inheritdoc/>
        public LanguageIf Where { get; set; }
    }
}
