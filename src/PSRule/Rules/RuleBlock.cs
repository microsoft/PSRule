// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    internal delegate bool RulePrecondition();

    internal delegate RuleConditionResult RuleCondition();

    /// <summary>
    /// Define an instance of a rule block. Each rule block has a unique id.
    /// </summary>
    [DebuggerDisplay("{Id} @{Source.Path}")]
    internal sealed class RuleBlock : ILanguageBlock, IDependencyTarget, IDisposable, IResource, IRuleV1
    {
        internal RuleBlock(SourceFile source, ResourceId id, ResourceId? @ref, RuleHelpInfo info, ICondition condition, ResourceTags tag, ResourceId[] alias, ResourceId[] dependsOn, Hashtable configuration, RuleExtent extent, ResourceFlags flags)
        {
            Source = source;
            Name = id.Name;

            // Get fully qualified Id, either RuleName or Module\RuleName
            Id = id;
            Ref = @ref;
            Alias = alias;

            Info = info;
            Condition = condition;
            Tag = tag;
            DependsOn = dependsOn;
            Configuration = configuration;
            Extent = extent;
            Flags = flags;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        public ResourceId Id { get; }

        public ResourceId? Ref { get; }

        public ResourceId[] Alias { get; }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The body of the rule definition where conditions are provided that either pass or fail the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public readonly ICondition Condition;

        /// <summary>
        /// Other rules that must completed successfully before calling this rule.
        /// </summary>
        public readonly ResourceId[] DependsOn;

        /// <summary>
        /// Tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        public readonly ResourceTags Tag;

        /// <summary>
        /// Configuration defaults for the rule definition.
        /// </summary>
        /// <remarks>
        /// These defaults are used when the value does not exist in the baseline configuration.
        /// </remarks>
        public readonly Hashtable Configuration;

        public readonly RuleHelpInfo Info;

        public readonly SourceFile Source;

        internal readonly RuleExtent Extent;

        [JsonIgnore]
        [YamlIgnore]
        public ResourceFlags Flags { get; }

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

        ResourceId[] IDependencyTarget.DependsOn => DependsOn;

        bool IDependencyTarget.Dependency => Source.IsDependency();

        ResourceKind IResource.Kind => ResourceKind.Rule;

        string IResource.ApiVersion => Specs.V1;

        string IResource.Name => Name;

        ResourceTags IResource.Tags => Tag;

        string IRuleV1.RuleName => Name;

        ResourceTags IRuleV1.Tag => Tag;

        string IRuleV1.Synopsis => Info.Synopsis;

        string IRuleV1.Description => Info.Synopsis;

        SourceFile IRuleV1.Source => Source;

        #region IDisposable

        public void Dispose()
        {
            Condition.Dispose();
        }

        #endregion IDisposable
    }
}
