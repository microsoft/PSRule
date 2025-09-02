// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSRule.Commands;
using PSRule.Options;

namespace PSRule.Host;

internal static class HostState
{
    /// <summary>
    /// Define language commands.
    /// </summary>
    private static readonly SessionStateCmdletEntry[] BuiltInCmdlets =
    [
        new("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
        new("Export-PSRuleConvention", typeof(ExportConventionCommand), null),
        new("Write-Recommendation", typeof(WriteRecommendationCommand), null),
        new("Write-Reason", typeof(WriteReasonCommand), null),
        new("Assert-Exists", typeof(AssertExistsCommand), null),
        new("Assert-Within", typeof(AssertWithinCommand), null),
        new("Assert-Match", typeof(AssertMatchCommand), null),
        new("Assert-TypeOf", typeof(AssertTypeOfCommand), null),
        new("Assert-AllOf", typeof(AssertAllOfCommand), null),
        new("Assert-AnyOf", typeof(AssertAnyOfCommand), null),
    ];

    /// <summary>
    /// Define language aliases.
    /// </summary>
    private static readonly SessionStateAliasEntry[] BuiltInAliases =
    [
        new(LanguageKeywords.Rule, "New-RuleDefinition", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.Recommend, "Write-Recommendation", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.Reason, "Write-Reason", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.Exists, "Assert-Exists", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.Within, "Assert-Within", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.Match, "Assert-Match", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.TypeOf, "Assert-TypeOf", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.AllOf, "Assert-AllOf", string.Empty, ScopedItemOptions.ReadOnly),
        new(LanguageKeywords.AnyOf, "Assert-AnyOf", string.Empty, ScopedItemOptions.ReadOnly),
    ];

    /// <summary>
    /// Create a default session state.
    /// </summary>
    public static InitialSessionState CreateSessionState(Options.SessionState initialSessionState, LanguageMode languageMode)
    {
        var state = initialSessionState == Options.SessionState.Minimal ?
            InitialSessionState.CreateDefault2() : InitialSessionState.CreateDefault();

        // Add in language elements
        state.Commands.Add(BuiltInCmdlets);
        state.Commands.Add(BuiltInAliases);

        // Set thread options
        state.ThreadOptions = PSThreadOptions.UseCurrentThread;

        // Set execution policy
        SetExecutionPolicy(state, executionPolicy: Microsoft.PowerShell.ExecutionPolicy.RemoteSigned);

        // Set the language mode.
        state.LanguageMode = languageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

        return state;
    }

    private static void SetExecutionPolicy(InitialSessionState state, Microsoft.PowerShell.ExecutionPolicy executionPolicy)
    {
        // Only set execution policy on Windows
        if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            state.ExecutionPolicy = executionPolicy;
    }
}
