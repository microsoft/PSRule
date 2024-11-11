// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Options;

/// <summary>
/// Options that configure the execution sandbox.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class ExecutionOption : IEquatable<ExecutionOption>, IExecutionOption
{
    private const BreakLevel DEFAULT_BREAK = BreakLevel.OnError;
    private const LanguageMode DEFAULT_LANGUAGEMODE = Options.LanguageMode.FullLanguage;
    private const ExecutionActionPreference DEFAULT_DUPLICATERESOURCEID = ExecutionActionPreference.Error;
    private const SessionState DEFAULT_INITIALSESSIONSTATE = SessionState.BuiltIn;
    private const RestrictScriptSource DEFAULT_RESTRICTSCRIPTSOURCE = Options.RestrictScriptSource.Unrestricted;
    private const ExecutionActionPreference DEFAULT_SUPPRESSIONGROUPEXPIRED = ExecutionActionPreference.Warn;
    private const ExecutionActionPreference DEFAULT_RULEEXCLUDED = ExecutionActionPreference.Ignore;
    private const ExecutionActionPreference DEFAULT_RULESUPPRESSED = ExecutionActionPreference.Warn;
    private const ExecutionActionPreference DEFAULT_ALIASREFERENCE = ExecutionActionPreference.Warn;
    private const ExecutionActionPreference DEFAULT_RULEINCONCLUSIVE = ExecutionActionPreference.Warn;
    private const ExecutionActionPreference DEFAULT_INVARIANTCULTURE = ExecutionActionPreference.Warn;
    private const ExecutionActionPreference DEFAULT_UNPROCESSEDOBJECT = ExecutionActionPreference.Warn;
    private const HashAlgorithm DEFAULT_HASHALGORITHM = Options.HashAlgorithm.SHA512;

    internal static readonly ExecutionOption Default = new()
    {
        Break = DEFAULT_BREAK,
        DuplicateResourceId = DEFAULT_DUPLICATERESOURCEID,
        HashAlgorithm = DEFAULT_HASHALGORITHM,
        LanguageMode = DEFAULT_LANGUAGEMODE,
        InitialSessionState = DEFAULT_INITIALSESSIONSTATE,
        RestrictScriptSource = DEFAULT_RESTRICTSCRIPTSOURCE,
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
        Break = null;
        DuplicateResourceId = null;
        HashAlgorithm = null;
        LanguageMode = null;
        InitialSessionState = null;
        RestrictScriptSource = null;
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

        Break = option.Break;
        DuplicateResourceId = option.DuplicateResourceId;
        HashAlgorithm = option.HashAlgorithm;
        LanguageMode = option.LanguageMode;
        InitialSessionState = option.InitialSessionState;
        RestrictScriptSource = option.RestrictScriptSource;
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
        return other != null &&
            Break == other.Break &&
            DuplicateResourceId == other.DuplicateResourceId &&
            HashAlgorithm == other.HashAlgorithm &&
            LanguageMode == other.LanguageMode &&
            InitialSessionState == other.InitialSessionState &&
            RestrictScriptSource == other.RestrictScriptSource &&
            SuppressionGroupExpired == other.SuppressionGroupExpired &&
            RuleExcluded == other.RuleExcluded &&
            RuleSuppressed == other.RuleSuppressed &&
            AliasReference == other.AliasReference &&
            RuleInconclusive == other.RuleInconclusive &&
            InvariantCulture == other.InvariantCulture &&
            UnprocessedObject == other.UnprocessedObject;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Break.HasValue ? Break.Value.GetHashCode() : 0);
            hash = hash * 23 + (DuplicateResourceId.HasValue ? DuplicateResourceId.Value.GetHashCode() : 0);
            hash = hash * 23 + (HashAlgorithm.HasValue ? HashAlgorithm.Value.GetHashCode() : 0);
            hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
            hash = hash * 23 + (InitialSessionState.HasValue ? InitialSessionState.Value.GetHashCode() : 0);
            hash = hash * 23 + (RestrictScriptSource.HasValue ? RestrictScriptSource.Value.GetHashCode() : 0);
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
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
    {
        var result = new ExecutionOption(o1)
        {
            Break = o1?.Break ?? o2?.Break,
            DuplicateResourceId = o1?.DuplicateResourceId ?? o2?.DuplicateResourceId,
            HashAlgorithm = o1?.HashAlgorithm ?? o2?.HashAlgorithm,
            LanguageMode = o1?.LanguageMode ?? o2?.LanguageMode,
            InitialSessionState = o1?.InitialSessionState ?? o2?.InitialSessionState,
            RestrictScriptSource = o1?.RestrictScriptSource ?? o2?.RestrictScriptSource,
            SuppressionGroupExpired = o1?.SuppressionGroupExpired ?? o2?.SuppressionGroupExpired,
            RuleExcluded = o1?.RuleExcluded ?? o2?.RuleExcluded,
            RuleSuppressed = o1?.RuleSuppressed ?? o2?.RuleSuppressed,
            AliasReference = o1?.AliasReference ?? o2?.AliasReference,
            RuleInconclusive = o1?.RuleInconclusive ?? o2?.RuleInconclusive,
            InvariantCulture = o1?.InvariantCulture ?? o2?.InvariantCulture,
            UnprocessedObject = o1?.UnprocessedObject ?? o2?.UnprocessedObject,
        };
        return result;
    }

    /// <summary>
    /// Determines the minimum rule severity level that breaks the pipeline.
    /// By default, the pipeline will break if a rule of error severity fails.
    /// </summary>
    [DefaultValue(null)]
    public BreakLevel? Break { get; set; }

    /// <summary>
    /// Determines how to handle duplicate resources identifiers during execution.
    /// Regardless of the value, only the first resource will be used.
    /// By default, an error is thrown.
    /// When set to Warn, a warning is generated.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? DuplicateResourceId { get; set; }

    /// <summary>
    /// Configures the hashing algorithm used by the PSRule runtime.
    /// </summary>
    [DefaultValue(null)]
    public HashAlgorithm? HashAlgorithm { get; set; }

    /// <summary>
    /// The language mode to execute PowerShell code with.
    /// </summary>
    [DefaultValue(null)]
    public LanguageMode? LanguageMode { get; set; }

    /// <summary>
    /// Determines how the initial session state for executing PowerShell code is created.
    /// The default is <see cref="SessionState.BuiltIn"/>.
    /// </summary>
    [DefaultValue(null)]
    public SessionState? InitialSessionState { get; set; }

    /// <summary>
    /// Configures where to allow PowerShell language features (such as rules and conventions) to run from.
    /// The default is <see cref="RestrictScriptSource.Unrestricted"/>.
    /// </summary>
    [DefaultValue(null)]
    public RestrictScriptSource? RestrictScriptSource { get; set; }

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
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? RuleSuppressed { get; set; }

    /// <summary>
    /// Determines how to handle when an alias to a resource is used.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? AliasReference { get; set; }

    /// <summary>
    /// Determines how to handle rules that generate inconclusive results.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? RuleInconclusive { get; set; }

    /// <summary>
    /// Determines how to report when an invariant culture is used.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? InvariantCulture { get; set; }

    /// <summary>
    /// Determines how to report objects that are not processed by any rule.
    /// By default, a warning is generated.
    /// When set to Error, an error is thrown.
    /// When set to Debug, a message is written to the debug log.
    /// When set to Ignore, no output will be displayed.
    /// </summary>
    [DefaultValue(null)]
    public ExecutionActionPreference? UnprocessedObject { get; set; }

    #region IExecutionOption

    BreakLevel IExecutionOption.Break => Break ?? DEFAULT_BREAK;

    ExecutionActionPreference IExecutionOption.DuplicateResourceId => DuplicateResourceId ?? DEFAULT_DUPLICATERESOURCEID;

    HashAlgorithm IExecutionOption.HashAlgorithm => HashAlgorithm ?? DEFAULT_HASHALGORITHM;

    LanguageMode IExecutionOption.LanguageMode => LanguageMode ?? DEFAULT_LANGUAGEMODE;

    SessionState IExecutionOption.InitialSessionState => InitialSessionState ?? DEFAULT_INITIALSESSIONSTATE;

    RestrictScriptSource IExecutionOption.RestrictScriptSource => RestrictScriptSource ?? DEFAULT_RESTRICTSCRIPTSOURCE;

    ExecutionActionPreference IExecutionOption.SuppressionGroupExpired => SuppressionGroupExpired ?? DEFAULT_SUPPRESSIONGROUPEXPIRED;

    ExecutionActionPreference IExecutionOption.RuleExcluded => RuleExcluded ?? DEFAULT_RULEEXCLUDED;

    ExecutionActionPreference IExecutionOption.RuleSuppressed => RuleSuppressed ?? DEFAULT_RULESUPPRESSED;

    ExecutionActionPreference IExecutionOption.AliasReference => AliasReference ?? DEFAULT_ALIASREFERENCE;

    ExecutionActionPreference IExecutionOption.RuleInconclusive => RuleInconclusive ?? DEFAULT_RULEINCONCLUSIVE;

    ExecutionActionPreference IExecutionOption.InvariantCulture => InvariantCulture ?? DEFAULT_INVARIANTCULTURE;

    ExecutionActionPreference IExecutionOption.UnprocessedObject => UnprocessedObject ?? DEFAULT_UNPROCESSEDOBJECT;

    #endregion IExecutionOption

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        if (Environment.TryEnum("PSRULE_EXECUTION_BREAK", out BreakLevel @break))
            Break = @break;

        if (Environment.TryEnum("PSRULE_EXECUTION_HASHALGORITHM", out HashAlgorithm hashAlgorithm))
            HashAlgorithm = hashAlgorithm;

        if (Environment.TryEnum("PSRULE_EXECUTION_DUPLICATERESOURCEID", out ExecutionActionPreference duplicateResourceId))
            DuplicateResourceId = duplicateResourceId;

        if (Environment.TryEnum("PSRULE_EXECUTION_LANGUAGEMODE", out LanguageMode languageMode))
            LanguageMode = languageMode;

        if (Environment.TryEnum("PSRULE_EXECUTION_INITIALSESSIONSTATE", out SessionState initialSessionState))
            InitialSessionState = initialSessionState;

        if (Environment.TryEnum("PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE", out RestrictScriptSource restrictScriptSource))
            RestrictScriptSource = restrictScriptSource;

        if (Environment.TryEnum("PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED", out ExecutionActionPreference suppressionGroupExpired))
            SuppressionGroupExpired = suppressionGroupExpired;

        if (Environment.TryEnum("PSRULE_EXECUTION_RULEEXCLUDED", out ExecutionActionPreference ruleExcluded))
            RuleExcluded = ruleExcluded;

        if (Environment.TryEnum("PSRULE_EXECUTION_RULESUPPRESSED", out ExecutionActionPreference ruleSuppressed))
            RuleSuppressed = ruleSuppressed;

        if (Environment.TryEnum("PSRULE_EXECUTION_ALIASREFERENCE", out ExecutionActionPreference aliasReference))
            AliasReference = aliasReference;

        if (Environment.TryEnum("PSRULE_EXECUTION_RULEINCONCLUSIVE", out ExecutionActionPreference ruleInconclusive))
            RuleInconclusive = ruleInconclusive;

        if (Environment.TryEnum("PSRULE_EXECUTION_INVARIANTCULTURE", out ExecutionActionPreference invariantCulture))
            InvariantCulture = invariantCulture;

        if (Environment.TryEnum("PSRULE_EXECUTION_UNPROCESSEDOBJECT", out ExecutionActionPreference unprocessedObject))
            UnprocessedObject = unprocessedObject;
    }

    /// <inheritdoc/>
    public void Import(IDictionary<string, object> dictionary)
    {
        if (dictionary.TryPopEnum("Execution.Break", out BreakLevel @break))
            Break = @break;

        if (dictionary.TryPopEnum("Execution.HashAlgorithm", out HashAlgorithm hashAlgorithm))
            HashAlgorithm = hashAlgorithm;

        if (dictionary.TryPopEnum("Execution.DuplicateResourceId", out ExecutionActionPreference duplicateResourceId))
            DuplicateResourceId = duplicateResourceId;

        if (dictionary.TryPopEnum("Execution.LanguageMode", out LanguageMode languageMode))
            LanguageMode = languageMode;

        if (dictionary.TryPopEnum("Execution.InitialSessionState", out SessionState initialSessionState))
            InitialSessionState = initialSessionState;

        if (dictionary.TryPopEnum("Execution.RestrictScriptSource", out RestrictScriptSource restrictScriptSource))
            RestrictScriptSource = restrictScriptSource;

        if (dictionary.TryPopEnum("Execution.SuppressionGroupExpired", out ExecutionActionPreference suppressionGroupExpired))
            SuppressionGroupExpired = suppressionGroupExpired;

        if (dictionary.TryPopEnum("Execution.RuleExcluded", out ExecutionActionPreference ruleExcluded))
            RuleExcluded = ruleExcluded;

        if (dictionary.TryPopEnum("Execution.RuleSuppressed", out ExecutionActionPreference ruleSuppressed))
            RuleSuppressed = ruleSuppressed;

        if (dictionary.TryPopEnum("Execution.AliasReference", out ExecutionActionPreference aliasReference))
            AliasReference = aliasReference;

        if (dictionary.TryPopEnum("Execution.RuleInconclusive", out ExecutionActionPreference ruleInconclusive))
            RuleInconclusive = ruleInconclusive;

        if (dictionary.TryPopEnum("Execution.InvariantCulture", out ExecutionActionPreference invariantCulture))
            InvariantCulture = invariantCulture;

        if (dictionary.TryPopEnum("Execution.UnprocessedObject", out ExecutionActionPreference unprocessedObject))
            UnprocessedObject = unprocessedObject;
    }
}
