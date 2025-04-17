// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Options;

/// <summary>
/// Options that configure the execution sandbox.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IExecutionOption : IOption
{
    /// <summary>
    /// Determines the minimum rule severity level that breaks the pipeline.
    /// By default, the pipeline will break if a rule of error severity fails.
    /// </summary>
    BreakLevel Break { get; }

    /// <summary>
    /// Determines how to handle duplicate resources identifiers during execution.
    /// Regardless of the value, only the first resource will be used.
    /// By default, an error is thrown.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference DuplicateResourceId { get; }

    /// <summary>
    /// Configures the hashing algorithm used by the PSRule runtime.
    /// The default is <see cref="HashAlgorithm.SHA512"/>.
    /// </summary>
    HashAlgorithm HashAlgorithm { get; }

    /// <summary>
    /// The language mode to execute PowerShell code with.
    /// The default is <see cref="LanguageMode.FullLanguage"/>.
    /// </summary>
    LanguageMode LanguageMode { get; }

    /// <summary>
    /// Determines how the initial session state for executing PowerShell code is created.
    /// The default is <see cref="SessionState.BuiltIn"/>.
    /// </summary>
    SessionState InitialSessionState { get; }

    /// <summary>
    /// Determines how to report cases when no rules are found.
    /// By default, an error is generated.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference NoMatchingRules { get; }

    /// <summary>
    /// Determines how to report cases when no valid input is found.
    /// By default, an error is generated.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference NoValidInput { get; }

    /// <summary>
    /// Determines how to report cases when no valid sources are found.
    /// By default, an error is generated.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference NoValidSources { get; }

    /// <summary>
    /// Configures where to allow PowerShell language features (such as rules and conventions) to run from.
    /// The default is <see cref="RestrictScriptSource.Unrestricted"/>.
    /// </summary>
    RestrictScriptSource RestrictScriptSource { get; }

    /// <summary>
    /// Determines how to handle expired suppression groups.
    /// Regardless of the value, an expired suppression group will be ignored.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference SuppressionGroupExpired { get; }

    /// <summary>
    /// Determines how to handle rules that are excluded.
    /// By default, excluded rules do not generated any output.
    /// When set to Error, an error is thrown.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// </summary>
    ExecutionActionPreference RuleExcluded { get; }

    /// <summary>
    /// Determines how to handle rules that are suppressed.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference RuleSuppressed { get; }

    /// <summary>
    /// Determines how to handle when an alias to a resource is used.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference AliasReference { get; }

    /// <summary>
    /// Determines how to handle rules that generate inconclusive results.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference RuleInconclusive { get; }

    /// <summary>
    /// Determines how to report when an invariant culture is used.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference InvariantCulture { get; }

    /// <summary>
    /// Determines how to report objects that are not processed by any rule.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    ExecutionActionPreference UnprocessedObject { get; }
}
