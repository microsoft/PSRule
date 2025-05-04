// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Definitions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Define a context used for early stage resource discovery.
/// </summary>
internal sealed class ResourceCacheDiscoveryContext(ILogger? logger, ILanguageScopeSet languageScopeSet) : IResourceDiscoveryContext
{
    private static readonly EventId PSR0022 = new(22, "PSR0022");

    private readonly ILanguageScopeSet _LanguageScopeSet = languageScopeSet;

    private ILanguageScope? _CurrentLanguageScope;

    public ILogger Logger { get; } = logger ?? NullLogger.Instance;

    public ISourceFile? Source { get; private set; }

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

    public void PopScope(RunspaceScope scope)
    {

    }

    public void PushScope(RunspaceScope scope)
    {

    }
}

#nullable restore
