// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that configure the execution sandbox.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options"/>.
    /// </remarks>
    public sealed class ExecutionOption : IEquatable<ExecutionOption>
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;
        private const ExecutionActionPreference DEFAULT_DUPLICATERESOURCEID = ExecutionActionPreference.Error;
        private const SessionState DEFAULT_INITIALSESSIONSTATE = SessionState.BuiltIn;
        private const ExecutionActionPreference DEFAULT_SUPPRESSIONGROUPEXPIRED = ExecutionActionPreference.Warn;
        private const ExecutionActionPreference DEFAULT_RULEEXCLUDED = ExecutionActionPreference.Ignore;
        private const ExecutionActionPreference DEFAULT_RULESUPPRESSED = ExecutionActionPreference.Warn;
        private const ExecutionActionPreference DEFAULT_ALIASREFERENCE = ExecutionActionPreference.Warn;
        private const ExecutionActionPreference DEFAULT_RULEINCONCLUSIVE = ExecutionActionPreference.Warn;
        private const ExecutionActionPreference DEFAULT_INVARIANTCULTURE = ExecutionActionPreference.Warn;
        private const ExecutionActionPreference DEFAULT_UNPROCESSEDOBJECT = ExecutionActionPreference.Warn;

        internal static readonly ExecutionOption Default = new()
        {
            DuplicateResourceId = DEFAULT_DUPLICATERESOURCEID,
            LanguageMode = DEFAULT_LANGUAGEMODE,
            InitialSessionState = DEFAULT_INITIALSESSIONSTATE,
            SuppressionGroupExpired = DEFAULT_SUPPRESSIONGROUPEXPIRED,
            RuleExcluded = DEFAULT_RULEEXCLUDED,
            RuleSuppressed = DEFAULT_RULESUPPRESSED,
            AliasReference = DEFAULT_ALIASREFERENCE,
            RuleInconclusive = DEFAULT_RULEINCONCLUSIVE,
            InvariantCulture = DEFAULT_INVARIANTCULTURE,
            UnprocessedObject = DEFAULT_UNPROCESSEDOBJECT,
        };

        /// <summary>
        /// Creates an empty execution option.
        /// </summary>
        public ExecutionOption()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            AliasReferenceWarning = null;
            DuplicateResourceId = null;
            LanguageMode = null;
            InconclusiveWarning = null;
            InvariantCultureWarning = null;
            InitialSessionState = null;
            NotProcessedWarning = null;
            SuppressedRuleWarning = null;
#pragma warning restore CS0618 // Type or member is obsolete
            SuppressionGroupExpired = null;
            RuleExcluded = null;
            RuleSuppressed = null;
            AliasReference = null;
            RuleInconclusive = null;
            InvariantCulture = null;
            UnprocessedObject = null;
        }

        /// <summary>
        /// Creates a execution option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public ExecutionOption(ExecutionOption option)
        {
            if (option == null)
                return;

#pragma warning disable CS0618 // Type or member is obsolete
            AliasReferenceWarning = option.AliasReferenceWarning;
            DuplicateResourceId = option.DuplicateResourceId;
            LanguageMode = option.LanguageMode;
            InconclusiveWarning = option.InconclusiveWarning;
            InvariantCultureWarning = option.InvariantCultureWarning;
            InitialSessionState = option.InitialSessionState;
            NotProcessedWarning = option.NotProcessedWarning;
            SuppressedRuleWarning = option.SuppressedRuleWarning;
#pragma warning restore CS0618 // Type or member is obsolete
            SuppressionGroupExpired = option.SuppressionGroupExpired;
            RuleExcluded = option.RuleExcluded;
            RuleSuppressed = option.RuleSuppressed;
            AliasReference = option.AliasReference;
            RuleInconclusive = option.RuleInconclusive;
            InvariantCulture = option.InvariantCulture;
            UnprocessedObject = option.UnprocessedObject;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ExecutionOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(ExecutionOption other)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return other != null &&
                AliasReferenceWarning == other.AliasReferenceWarning &&
                DuplicateResourceId == other.DuplicateResourceId &&
                LanguageMode == other.LanguageMode &&
                InconclusiveWarning == other.InconclusiveWarning &&
                InvariantCultureWarning == other.InvariantCultureWarning &&
                InitialSessionState == other.InitialSessionState &&
                NotProcessedWarning == other.NotProcessedWarning &&
                SuppressedRuleWarning == other.SuppressedRuleWarning &&
                SuppressionGroupExpired == other.SuppressionGroupExpired &&
                RuleExcluded == other.RuleExcluded &&
                RuleSuppressed == other.RuleSuppressed &&
                AliasReference == other.AliasReference &&
                RuleInconclusive == other.RuleInconclusive &&
                InvariantCulture == other.InvariantCulture &&
                UnprocessedObject == other.UnprocessedObject;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
#pragma warning disable CS0618 // Type or member is obsolete
                hash = hash * 23 + (AliasReferenceWarning.HasValue ? AliasReferenceWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (DuplicateResourceId.HasValue ? DuplicateResourceId.Value.GetHashCode() : 0);
                hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
                hash = hash * 23 + (InconclusiveWarning.HasValue ? InconclusiveWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (InvariantCultureWarning.HasValue ? InvariantCultureWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (InitialSessionState.HasValue ? InitialSessionState.Value.GetHashCode() : 0);
                hash = hash * 23 + (NotProcessedWarning.HasValue ? NotProcessedWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (SuppressedRuleWarning.HasValue ? SuppressedRuleWarning.Value.GetHashCode() : 0);
#pragma warning restore CS0618 // Type or member is obsolete
                hash = hash * 23 + (SuppressionGroupExpired.HasValue ? SuppressionGroupExpired.Value.GetHashCode() : 0);
                hash = hash * 23 + (RuleExcluded.HasValue ? RuleExcluded.Value.GetHashCode() : 0);
                hash = hash * 23 + (RuleSuppressed.HasValue ? RuleSuppressed.Value.GetHashCode() : 0);
                hash = hash * 23 + (AliasReference.HasValue ? AliasReference.Value.GetHashCode() : 0);
                hash = hash * 23 + (RuleInconclusive.HasValue ? RuleInconclusive.Value.GetHashCode() : 0);
                hash = hash * 23 + (InvariantCulture.HasValue ? InvariantCulture.Value.GetHashCode() : 0);
                hash = hash * 23 + (UnprocessedObject.HasValue ? UnprocessedObject.Value.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = new ExecutionOption(o1)
            {
                AliasReferenceWarning = o1.AliasReferenceWarning ?? o2.AliasReferenceWarning,
                DuplicateResourceId = o1.DuplicateResourceId ?? o2.DuplicateResourceId,
                LanguageMode = o1.LanguageMode ?? o2.LanguageMode,
                InconclusiveWarning = o1.InconclusiveWarning ?? o2.InconclusiveWarning,
                NotProcessedWarning = o1.NotProcessedWarning ?? o2.NotProcessedWarning,
                SuppressedRuleWarning = o1.SuppressedRuleWarning ?? o2.SuppressedRuleWarning,
                InvariantCultureWarning = o1.InvariantCultureWarning ?? o2.InvariantCultureWarning,
                InitialSessionState = o1.InitialSessionState ?? o2.InitialSessionState,
                SuppressionGroupExpired = o1.SuppressionGroupExpired ?? o2.SuppressionGroupExpired,
                RuleExcluded = o1.RuleExcluded ?? o2.RuleExcluded,
                RuleSuppressed = o1.RuleSuppressed ?? o2.RuleSuppressed,
                AliasReference = o1.AliasReference ?? o2.AliasReference,
                RuleInconclusive = o1.RuleInconclusive ?? o2.RuleInconclusive,
                InvariantCulture = o1.InvariantCulture ?? o2.InvariantCulture,
                UnprocessedObject = o1.UnprocessedObject ?? o2.UnprocessedObject,
            };
#pragma warning restore CS0618 // Type or member is obsolete
            return result;
        }

        /// <summary>
        /// Determines if a warning is raised when an alias to a resource is used.
        /// </summary>
        [DefaultValue(null), Obsolete("Use AliasReference instead. See https://aka.ms/ps-rule/deprecations for more detail.")]
        public bool? AliasReferenceWarning { get; set; }

        /// <summary>
        /// Determines how to handle duplicate resources identifiers during execution.
        /// Regardless of the value, only the first resource will be used.
        /// By defaut, an error is thrown.
        /// When set to Warn, a warning is generated.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        [DefaultValue(null)]
        public ExecutionActionPreference? DuplicateResourceId { get; set; }

        /// <summary>
        /// The langauge mode to execute PowerShell code with.
        /// </summary>
        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }

        /// <summary>
        /// Determines if a warning is raised when a rule does not return pass or fail.
        /// </summary>
        [DefaultValue(null), Obsolete("Use RuleInconclusive instead. See https://aka.ms/ps-rule/deprecations for more detail.")]
        public bool? InconclusiveWarning { get; set; }

        /// <summary>
        /// Determines if warning is raised when invariant culture is used.
        /// </summary>
        [DefaultValue(null), Obsolete("Use InvariantCulture instead. See https://aka.ms/ps-rule/deprecations for more detail.")]
        public bool? InvariantCultureWarning { get; set; }

        /// <summary>
        /// Determines how the initial session state for executing PowerShell code is created.
        /// The default is <see cref="SessionState.BuiltIn"/>.
        /// </summary>
        [DefaultValue(null)]
        public SessionState? InitialSessionState { get; set; }

        /// <summary>
        /// Determines if a warning is raised when an object is not processed by any rule.
        /// </summary>
        [DefaultValue(null), Obsolete("Use UnprocessedObject instead. See https://aka.ms/ps-rule/deprecations for more detail.")]
        public bool? NotProcessedWarning { get; set; }

        /// <summary>
        /// Determines how to handle expired suppression groups.
        /// Regardless of the value, an expired suppression group will be ignored.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        [DefaultValue(null)]
        public ExecutionActionPreference? SuppressionGroupExpired { get; set; }

        /// <summary>
        /// Determines if a warning is raised when a rule is suppressed.
        /// </summary>
        [DefaultValue(null), Obsolete("Use RuleSuppressed instead. See https://aka.ms/ps-rule/deprecations for more detail.")]
        public bool? SuppressedRuleWarning { get; set; }

        /// <summary>
        /// Determines how to handle rules that are excluded.
        /// By default, excluded rules do not generated any output.
        /// When set to Error, an error is thrown.
        /// When set to Warn, a warning is generated.
        /// When set to Debug, a message is written to the debug log.
        /// </summary>
        [DefaultValue(null)]
        public ExecutionActionPreference? RuleExcluded { get; set; }

        /// <summary>
        /// Determines how to handle rules that are suppressed.
        /// This option replaces <seealso cref="SuppressedRuleWarning"/>.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        /// <remarks>
        /// If <seealso cref="SuppressedRuleWarning"/> is <c>true</c> this option will be overridden to <c>Warn</c>.
        /// If <seealso cref="SuppressedRuleWarning"/> is <c>false</c> this option will be overridden to <c>Ignore</c>.
        /// </remarks>
        [DefaultValue(null)]
        public ExecutionActionPreference? RuleSuppressed { get; set; }

        /// <summary>
        /// Determines how to handle when an alias to a resource is used.
        /// This option replaces <seealso cref="AliasReferenceWarning"/>.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        /// <remarks>
        /// If <seealso cref="AliasReferenceWarning"/> is <c>true</c> this option will be overridden to <c>Warn</c>.
        /// If <seealso cref="AliasReferenceWarning"/> is <c>false</c> this option will be overridden to <c>Ignore</c>.
        /// </remarks>
        [DefaultValue(null)]
        public ExecutionActionPreference? AliasReference { get; set; }

        /// <summary>
        /// Determines how to handle rules that generate inconclusive results.
        /// This option replaces <seealso cref="InconclusiveWarning"/>.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        /// <remarks>
        /// If <seealso cref="InconclusiveWarning"/> is <c>true</c> this option will be overridden to <c>Warn</c>.
        /// If <seealso cref="InconclusiveWarning"/> is <c>false</c> this option will be overridden to <c>Ignore</c>.
        /// </remarks>
        [DefaultValue(null)]
        public ExecutionActionPreference? RuleInconclusive { get; set; }

        /// <summary>
        /// Determines how to report when an invariant culture is used.
        /// This option replaces <seealso cref="InvariantCultureWarning"/>.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        /// <remarks>
        /// If <seealso cref="InvariantCultureWarning"/> is <c>true</c> this option will be overridden to <c>Warn</c>.
        /// If <seealso cref="InvariantCultureWarning"/> is <c>false</c> this option will be overridden to <c>Ignore</c>.
        /// </remarks>
        [DefaultValue(null)]
        public ExecutionActionPreference? InvariantCulture { get; set; }

        /// <summary>
        /// Determines how to report objects that are not processed by any rule.
        /// This option replaces <seealso cref="NotProcessedWarning"/>.
        /// By default, a warning is generated.
        /// When set to Error, an error is thrown.
        /// When set to Debug, a message is written to the debug log.
        /// When set to Ignore, no output will be displayed.
        /// </summary>
        /// <remarks>
        /// If <seealso cref="NotProcessedWarning"/> is <c>true</c> this option will be overridden to <c>Warn</c>.
        /// If <seealso cref="NotProcessedWarning"/> is <c>false</c> this option will be overridden to <c>Ignore</c>.
        /// </remarks>
        [DefaultValue(null)]
        public ExecutionActionPreference? UnprocessedObject { get; set; }

        /// <summary>
        /// Load from environment variables.
        /// </summary>
        internal void Load(EnvironmentHelper env)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (env.TryBool("PSRULE_EXECUTION_ALIASREFERENCEWARNING", out var bvalue))
                AliasReferenceWarning = bvalue;

            if (env.TryEnum("PSRULE_EXECUTION_DUPLICATERESOURCEID", out ExecutionActionPreference duplicateResourceId))
                DuplicateResourceId = duplicateResourceId;

            if (env.TryEnum("PSRULE_EXECUTION_LANGUAGEMODE", out LanguageMode languageMode))
                LanguageMode = languageMode;

            if (env.TryBool("PSRULE_EXECUTION_INCONCLUSIVEWARNING", out bvalue))
                InconclusiveWarning = bvalue;

            if (env.TryBool("PSRULE_EXECUTION_INVARIANTCULTUREWARNING", out bvalue))
                InvariantCultureWarning = bvalue;

            if (env.TryEnum("PSRULE_EXECUTION_INITIALSESSIONSTATE", out SessionState initialSessionState))
                InitialSessionState = initialSessionState;

            if (env.TryBool("PSRULE_EXECUTION_NOTPROCESSEDWARNING", out bvalue))
                NotProcessedWarning = bvalue;

            if (env.TryBool("PSRULE_EXECUTION_SUPPRESSEDRULEWARNING", out bvalue))
                SuppressedRuleWarning = bvalue;
#pragma warning restore CS0618 // Type or member is obsolete

            if (env.TryEnum("PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED", out ExecutionActionPreference suppressionGroupExpired))
                SuppressionGroupExpired = suppressionGroupExpired;

            if (env.TryEnum("PSRULE_EXECUTION_RULEEXCLUDED", out ExecutionActionPreference ruleExcluded))
                RuleExcluded = ruleExcluded;

            if (env.TryEnum("PSRULE_EXECUTION_RULESUPPRESSED", out ExecutionActionPreference ruleSuppressed))
                RuleSuppressed = ruleSuppressed;

            if (env.TryEnum("PSRULE_EXECUTION_ALIASREFERENCE", out ExecutionActionPreference aliasReference))
                AliasReference = aliasReference;

            if (env.TryEnum("PSRULE_EXECUTION_RULEINCONCLUSIVE", out ExecutionActionPreference ruleInconclusive))
                RuleInconclusive = ruleInconclusive;

            if (env.TryEnum("PSRULE_EXECUTION_INVARIANTCULTURE", out ExecutionActionPreference invariantCulture))
                InvariantCulture = invariantCulture;

            if (env.TryEnum("PSRULE_EXECUTION_UNPROCESSEDOBJECT", out ExecutionActionPreference unprocessedObject))
                UnprocessedObject = unprocessedObject;
        }

        /// <summary>
        /// Load from dictionary.
        /// </summary>
        internal void Load(Dictionary<string, object> index)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (index.TryPopBool("Execution.AliasReferenceWarning", out var bvalue))
                AliasReferenceWarning = bvalue;

            if (index.TryPopEnum("Execution.DuplicateResourceId", out ExecutionActionPreference duplicateResourceId))
                DuplicateResourceId = duplicateResourceId;

            if (index.TryPopEnum("Execution.LanguageMode", out LanguageMode languageMode))
                LanguageMode = languageMode;

            if (index.TryPopBool("Execution.InconclusiveWarning", out bvalue))
                InconclusiveWarning = bvalue;

            if (index.TryPopBool("Execution.InvariantCultureWarning", out bvalue))
                InvariantCultureWarning = bvalue;

            if (index.TryPopEnum("Execution.InitialSessionState", out SessionState initialSessionState))
                InitialSessionState = initialSessionState;

            if (index.TryPopBool("Execution.NotProcessedWarning", out bvalue))
                NotProcessedWarning = bvalue;

            if (index.TryPopBool("Execution.SuppressedRuleWarning", out bvalue))
                SuppressedRuleWarning = bvalue;
#pragma warning restore CS0618 // Type or member is obsolete

            if (index.TryPopEnum("Execution.SuppressionGroupExpired", out ExecutionActionPreference suppressionGroupExpired))
                SuppressionGroupExpired = suppressionGroupExpired;

            if (index.TryPopEnum("Execution.RuleExcluded", out ExecutionActionPreference ruleExcluded))
                RuleExcluded = ruleExcluded;

            if (index.TryPopEnum("Execution.RuleSuppressed", out ExecutionActionPreference ruleSuppressed))
                RuleSuppressed = ruleSuppressed;

            if (index.TryPopEnum("Execution.AliasReference", out ExecutionActionPreference aliasReference))
                AliasReference = aliasReference;

            if (index.TryPopEnum("Execution.RuleInconclusive", out ExecutionActionPreference ruleInconclusive))
                RuleInconclusive = ruleInconclusive;

            if (index.TryPopEnum("Execution.InvariantCulture", out ExecutionActionPreference invariantCulture))
                InvariantCulture = invariantCulture;

            if (index.TryPopEnum("Execution.UnprocessedObject", out ExecutionActionPreference unprocessedObject))
                UnprocessedObject = unprocessedObject;
        }
    }
}
