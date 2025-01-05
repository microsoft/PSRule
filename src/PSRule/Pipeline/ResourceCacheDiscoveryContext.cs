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
internal sealed class ResourceCacheDiscoveryContext(IPipelineWriter? writer, ILanguageScopeSet languageScopeSet) : IResourceDiscoveryContext
{
    private readonly ILanguageScopeSet _LanguageScopeSet = languageScopeSet;

    private ILanguageScope? _CurrentLanguageScope;

    public IPipelineWriter? Writer { get; } = writer;

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
            throw new Exception("Language scope is unknown.");

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
