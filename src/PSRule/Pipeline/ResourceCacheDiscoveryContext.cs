// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Define a context used for early stage resource discovery.
/// </summary>
internal sealed class ResourceCacheDiscoveryContext(IPipelineWriter writer, ILanguageScopeSet languageScopeSet) : IResourceDiscoveryContext
{
    private readonly ExecutionActionPreference _InvariantCulture; // TODO set
    private readonly ILanguageScopeSet _LanguageScopeSet = languageScopeSet;

    private bool _RaisedUsingInvariantCulture = false;

    private ILanguageScope? _CurrentLanguageScope;

    public IPipelineWriter Writer { get; } = writer;

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

    public string? GetLocalizedPath(string file, out string? culture)
    {
        culture = null;
        if (string.IsNullOrEmpty(Source?.HelpPath))
            return null;

        var cultures = LanguageScope?.Culture;
        if (!_RaisedUsingInvariantCulture && (cultures == null || cultures.Length == 0))
        {
            Throw(_InvariantCulture, PSRuleResources.UsingInvariantCulture);
            _RaisedUsingInvariantCulture = true;
            return null;
        }

        for (var i = 0; cultures != null && i < cultures.Length; i++)
        {
            var path = Path.Combine(Source?.HelpPath, cultures[i], file);
            if (File.Exists(path))
            {
                culture = cultures[i];
                return path;
            }
        }
        return null;
    }

    private void Throw(ExecutionActionPreference action, string message, params object[] args)
    {
        if (action == ExecutionActionPreference.Ignore)
            return;

        if (action == ExecutionActionPreference.Error)
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, message, args));

        else if (action == ExecutionActionPreference.Warn && Writer != null && Writer.ShouldWriteWarning())
            Writer.WriteWarning(message, args);

        else if (action == ExecutionActionPreference.Debug && Writer != null && Writer.ShouldWriteDebug())
            Writer.WriteDebug(message, args);
    }
}

#nullable restore
