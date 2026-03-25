// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;
using PSRule.Runtime.Scripting;

namespace PSRule.Pipeline;

/// <summary>
/// Define a context used for early stage resource discovery.
/// </summary>
internal sealed class ResourceCacheDiscoveryContext(PSRuleOption option, ILogger? logger, IRunspaceContext? runspaceContext, ILanguageScopeSet languageScopeSet) : IScriptResourceDiscoveryContext
{
    private static readonly EventId PSR0022 = new(22, "PSR0022");

    private readonly ExecutionActionPreference _DuplicateResourceId = option?.Execution?.DuplicateResourceId ?? ExecutionOption.Default.DuplicateResourceId!.Value;
    private readonly ILanguageScopeSet _LanguageScopeSet = languageScopeSet;
    private readonly IRunspaceContext? _RunspaceContext = runspaceContext;
    private readonly ExecutionActionPreference _InvariantCulture;

    private bool _RaisedUsingInvariantCulture;

    private ILanguageScope? _CurrentLanguageScope;

    public ILogger Logger { get; } = logger ?? NullLogger.Instance;

    public ISourceFile? Source { get; private set; }

    public string? Scope => _CurrentLanguageScope?.Name;

    public RestrictScriptSource RestrictScriptSource => _RunspaceContext == null ? RestrictScriptSource.DisablePowerShell : _RunspaceContext.RestrictScriptSource;

    internal ILanguageScope? LanguageScope
    {
        [DebuggerStepThrough]
        get
        {
            return _CurrentLanguageScope;
        }
    }

    public void EnterLanguageScope(ISourceFile file)
    {
        if (!file.Exists())
            throw new FileNotFoundException(PSRuleResources.ScriptNotFound, file.Path);

        if (!_LanguageScopeSet.TryScope(file.Module, out var scope))
            throw new RuntimeScopeException(PSR0022, PSRuleResources.PSR0022);

        Source = file;
        _CurrentLanguageScope = scope;
    }

    public void ExitLanguageScope(ISourceFile file)
    {
        Source = null;
        _CurrentLanguageScope = null;
    }

    public PowerShell? GetPowerShell()
    {
        return _RunspaceContext?.GetPowerShell();
    }

    public void PopScope(RunspaceScope scope)
    {

    }

    public void PushScope(RunspaceScope scope)
    {

    }

    public bool IsScope(RunspaceScope scope)
    {
        return true;
    }

    public string? GetLocalizedPath(string file, out string? culture)
    {
        culture = null;
        if (string.IsNullOrEmpty(Source?.HelpPath))
            return null;

        var cultures = LanguageScope?.Culture;
        if (!_RaisedUsingInvariantCulture && (cultures == null || cultures.Length == 0))
        {
            Logger?.Throw(_InvariantCulture, PSRuleResources.UsingInvariantCulture);
            _RaisedUsingInvariantCulture = true;
            return null;
        }

        if (cultures == null || cultures.Length == 0)
            return null;

        return new LocalizedFileSearch(cultures).GetLocalizedPath(Source!.HelpPath, file, out culture);
    }

    public void ReportIssue(ResourceIssue issue)
    {
        switch (issue.Type)
        {
            case ResourceIssueType.DuplicateResourceId:
                Logger?.Throw(_DuplicateResourceId, PSRuleResources.DuplicateResourceId, issue.ResourceId, issue.Args![0]);
                break;
            case ResourceIssueType.DuplicateResourceName:
                Logger?.LogWarning(new EventId(0), PSRuleResources.DuplicateRuleName, issue.Args![0]);
                break;
            default:
                throw new NotImplementedException($"Resource issue '{issue.Type}' is not implemented.");
        }
    }

    internal void Begin()
    {
        _RunspaceContext?.EnterResourceContext(this);
    }

    internal void End()
    {
        _RunspaceContext?.ExitResourceContext(this);
    }
}
