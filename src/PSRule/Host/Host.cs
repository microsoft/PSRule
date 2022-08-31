// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSRule.Commands;
using PSRule.Runtime;

namespace PSRule.Host
{
    /// <summary>
    /// A dynamic variable $PSRule used during Rule execution.
    /// </summary>
    internal sealed class PSRuleVariable : PSVariable
    {
        private const string VARIABLE_NAME = "PSRule";

        private readonly Runtime.PSRule _Value;

        public PSRuleVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Runtime.PSRule(RunspaceContext.CurrentThread);
        }

        public override object Value => _Value;
    }

    /// <summary>
    /// A dynamic variable $Rule used during Rule execution.
    /// </summary>
    internal sealed class RuleVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Rule";

        private readonly Rule _Value;

        public RuleVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Rule();
        }

        public override object Value => _Value;
    }

    /// <summary>
    /// A dynamic variable $LocalizedData used during Rule execution.
    /// </summary>
    internal sealed class LocalizedDataVariable : PSVariable
    {
        private const string VARIABLE_NAME = "LocalizedData";

        private readonly LocalizedData _Value;

        public LocalizedDataVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new LocalizedData();
        }

        public override object Value => _Value;
    }

    /// <summary>
    /// An assertion helper variable $Assert used during Rule execution.
    /// </summary>
    internal sealed class AssertVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Assert";
        private readonly Assert _Value;

        public AssertVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Assert();
        }

        public override object Value => _Value;
    }

    /// <summary>
    /// A dynamic variable used during Rule execution.
    /// </summary>
    internal sealed class TargetObjectVariable : PSVariable
    {
        private const string VARIABLE_NAME = "TargetObject";

        public TargetObjectVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value => RunspaceContext.CurrentThread.TargetObject.Value;
    }

    internal sealed class ConfigurationVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Configuration";
        private readonly Runtime.Configuration _Value;

        public ConfigurationVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Runtime.Configuration(RunspaceContext.CurrentThread);
        }

        public override object Value => _Value;
    }

    internal static class HostState
    {
        /// <summary>
        /// Define language commands.
        /// </summary>
        private readonly static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
            new SessionStateCmdletEntry("Export-PSRuleConvention", typeof(ExportConventionCommand), null),
            new SessionStateCmdletEntry("Write-Recommendation", typeof(WriteRecommendationCommand), null),
            new SessionStateCmdletEntry("Write-Reason", typeof(WriteReasonCommand), null),
            new SessionStateCmdletEntry("Assert-Exists", typeof(AssertExistsCommand), null),
            new SessionStateCmdletEntry("Assert-Within", typeof(AssertWithinCommand), null),
            new SessionStateCmdletEntry("Assert-Match", typeof(AssertMatchCommand), null),
            new SessionStateCmdletEntry("Assert-TypeOf", typeof(AssertTypeOfCommand), null),
            new SessionStateCmdletEntry("Assert-AllOf", typeof(AssertAllOfCommand), null),
            new SessionStateCmdletEntry("Assert-AnyOf", typeof(AssertAnyOfCommand), null),
        };

        /// <summary>
        /// Define language aliases.
        /// </summary>
        private readonly static SessionStateAliasEntry[] BuiltInAliases = new SessionStateAliasEntry[]
        {
            new SessionStateAliasEntry(LanguageKeywords.Rule, "New-RuleDefinition", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Recommend, "Write-Recommendation", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Reason, "Write-Reason", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Exists, "Assert-Exists", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Within, "Assert-Within", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Match, "Assert-Match", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.TypeOf, "Assert-TypeOf", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.AllOf, "Assert-AllOf", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.AnyOf, "Assert-AnyOf", string.Empty, ScopedItemOptions.ReadOnly),
        };

        /// <summary>
        /// Create a default session state.
        /// </summary>
        public static InitialSessionState CreateSessionState(Configuration.SessionState initialSessionState)
        {
            var state = initialSessionState == Configuration.SessionState.Minimal ?
                InitialSessionState.CreateDefault2() : InitialSessionState.CreateDefault();

            // Add in language elements
            state.Commands.Add(BuiltInCmdlets);
            state.Commands.Add(BuiltInAliases);

            // Set thread options
            state.ThreadOptions = PSThreadOptions.UseCurrentThread;

            // Set execution policy
            SetExecutionPolicy(state, executionPolicy: Microsoft.PowerShell.ExecutionPolicy.RemoteSigned);

            return state;
        }

        private static void SetExecutionPolicy(InitialSessionState state, Microsoft.PowerShell.ExecutionPolicy executionPolicy)
        {
            // Only set execution policy on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                state.ExecutionPolicy = executionPolicy;
        }
    }
}
