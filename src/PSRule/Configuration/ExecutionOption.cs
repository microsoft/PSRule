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
        private const bool DEFAULT_INCONCLUSIVEWARNING = true;
        private const bool DEFAULT_NOTPROCESSEDWARNING = true;
        private const bool DEFAULT_SUPPRESSEDRULEWARNING = true;
        private const bool DEFAULT_ALIASREFERENCEWARNING = true;
        private const bool DEFAULT_INVARIANTCULTUREWARNING = true;
        private const ExecutionActionPreference DEFAULT_DUPLICATERESOURCEID = ExecutionActionPreference.Error;
        private const SessionState DEFAULT_INITIALSESSIONSTATE = SessionState.BuiltIn;
        private const ExecutionActionPreference DEFAULT_SUPPRESSIONGROUPEXPIRED = ExecutionActionPreference.Warn;

        internal static readonly ExecutionOption Default = new()
        {
            AliasReferenceWarning = DEFAULT_ALIASREFERENCEWARNING,
            DuplicateResourceId = DEFAULT_DUPLICATERESOURCEID,
            LanguageMode = DEFAULT_LANGUAGEMODE,
            InconclusiveWarning = DEFAULT_INCONCLUSIVEWARNING,
            InvariantCultureWarning = DEFAULT_INVARIANTCULTUREWARNING,
            InitialSessionState = DEFAULT_INITIALSESSIONSTATE,
            NotProcessedWarning = DEFAULT_NOTPROCESSEDWARNING,
            SuppressedRuleWarning = DEFAULT_SUPPRESSEDRULEWARNING,
            SuppressionGroupExpired = DEFAULT_SUPPRESSIONGROUPEXPIRED,
        };

        /// <summary>
        /// Creates an empty execution option.
        /// </summary>
        public ExecutionOption()
        {
            AliasReferenceWarning = null;
            DuplicateResourceId = null;
            LanguageMode = null;
            InconclusiveWarning = null;
            InvariantCultureWarning = null;
            InitialSessionState = null;
            NotProcessedWarning = null;
            SuppressedRuleWarning = null;
            SuppressionGroupExpired = null;
        }

        /// <summary>
        /// Creates a execution option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public ExecutionOption(ExecutionOption option)
        {
            if (option == null)
                return;

            AliasReferenceWarning = option.AliasReferenceWarning;
            DuplicateResourceId = option.DuplicateResourceId;
            LanguageMode = option.LanguageMode;
            InconclusiveWarning = option.InconclusiveWarning;
            InvariantCultureWarning = option.InvariantCultureWarning;
            InitialSessionState = option.InitialSessionState;
            NotProcessedWarning = option.NotProcessedWarning;
            SuppressedRuleWarning = option.SuppressedRuleWarning;
            SuppressionGroupExpired = option.SuppressionGroupExpired;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ExecutionOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(ExecutionOption other)
        {
            return other != null &&
                AliasReferenceWarning == other.AliasReferenceWarning &&
                DuplicateResourceId == other.DuplicateResourceId &&
                LanguageMode == other.LanguageMode &&
                InconclusiveWarning == other.InconclusiveWarning &&
                InvariantCultureWarning == other.InvariantCultureWarning &&
                InitialSessionState == other.InitialSessionState &&
                NotProcessedWarning == other.NotProcessedWarning &&
                SuppressedRuleWarning == other.NotProcessedWarning &&
                SuppressionGroupExpired == other.SuppressionGroupExpired;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (AliasReferenceWarning.HasValue ? AliasReferenceWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (DuplicateResourceId.HasValue ? DuplicateResourceId.Value.GetHashCode() : 0);
                hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
                hash = hash * 23 + (InconclusiveWarning.HasValue ? InconclusiveWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (InvariantCultureWarning.HasValue ? InvariantCultureWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (InitialSessionState.HasValue ? InitialSessionState.Value.GetHashCode() : 0);
                hash = hash * 23 + (NotProcessedWarning.HasValue ? NotProcessedWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (SuppressedRuleWarning.HasValue ? SuppressedRuleWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (SuppressionGroupExpired.HasValue ? SuppressionGroupExpired.Value.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
        {
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
            };
            return result;
        }

        /// <summary>
        /// Determines if a warning is raised when an alias to a resource is used.
        /// </summary>
        [DefaultValue(null)]
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
        [DefaultValue(null)]
        public bool? InconclusiveWarning { get; set; }

        /// <summary>
        /// Determines if warning is raised when invariant culture is used.
        /// </summary>
        [DefaultValue(null)]
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
        [DefaultValue(null)]
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
        [DefaultValue(null)]
        public bool? SuppressedRuleWarning { get; set; }

        internal void Load(EnvironmentHelper env)
        {
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

            if (env.TryEnum("PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED", out ExecutionActionPreference suppressionGroupExpired))
                SuppressionGroupExpired = suppressionGroupExpired;
        }

        internal void Load(Dictionary<string, object> index)
        {
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

            if (index.TryPopEnum("Execution.SuppressionGroupExpired", out ExecutionActionPreference suppressionGroupExpired))
                SuppressionGroupExpired = suppressionGroupExpired;
        }
    }
}
