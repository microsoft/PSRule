// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline.Runs;

internal class RunCollectionBuilderContext(ILogger logger, PSRuleOption? option, ILanguageScopeSet languageScopeSet, RuleFilter filter) : IRunBuilderContext
{
    private readonly Stack<ISourceFile> _Sources = [];
    private readonly ILanguageScopeSet _LanguageScopeSet = languageScopeSet ?? throw new ArgumentNullException(nameof(languageScopeSet));
    private readonly ExecutionActionPreference _InvariantCulture = option?.Execution?.InvariantCulture ?? ExecutionOption.Default.InvariantCulture!.Value;
    private readonly ExecutionActionPreference _DuplicateResourceId = option?.Execution?.DuplicateResourceId ?? ExecutionOption.Default.DuplicateResourceId!.Value;

    private bool _RaisedUsingInvariantCulture;

    private ILanguageScope? _Scope;

    public ILogger? Logger => logger;

    public ISourceFile? Source => _Sources.Count > 0 ? _Sources.Peek() : null;

    string? IResourceContext.Scope => _Scope?.Name;

    public void EnterLanguageScope(ISourceFile file)
    {
        _Sources.Push(file);
        _Scope = _LanguageScopeSet.TryScope(file.Module, out var scope) ? scope : null;

    }

    public void ExitLanguageScope(ISourceFile file)
    {
        if (_Sources.Count == 0 || _Sources.Peek() != file)
            throw new InvalidOperationException();

        _Sources.Pop();
        _Scope = _Sources.Count > 0 && _LanguageScopeSet.TryScope(_Sources.Peek().Module, out var scope) ? scope : null;
    }

    public string? GetLocalizedPath(string file, out string? culture)
    {
        culture = null;
        if (string.IsNullOrEmpty(Source?.HelpPath))
            return null;

        var cultures = _Scope?.Culture;
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

    public bool Match(IResource resource)
    {
        return filter.Match(resource);
    }

    public void ReportIssue(ResourceIssue resourceIssue)
    {
        switch (resourceIssue.Type)
        {
            case ResourceIssueType.DuplicateResourceId:
                Logger?.Throw(_DuplicateResourceId, PSRuleResources.DuplicateResourceId, resourceIssue.ResourceId, resourceIssue.Args![0]);
                break;
            case ResourceIssueType.DuplicateResourceName:
                Logger?.LogWarning(new EventId(0), PSRuleResources.DuplicateRuleName, resourceIssue.Args![0]);
                break;
            case ResourceIssueType.RuleExcluded:
                var preference1 = option?.Execution?.RuleExcluded ?? ExecutionOption.Default.RuleExcluded!.Value;
                Logger?.Throw(preference1, PSRuleResources.RuleExcluded, resourceIssue.ResourceId.Value);
                break;
            case ResourceIssueType.AliasReference:
                var preference2 = option?.Execution?.AliasReference ?? ExecutionOption.Default.AliasReference!.Value;
                Logger?.Throw(preference2, PSRuleResources.AliasReference, resourceIssue?.Args[1], resourceIssue.ResourceId.Value, resourceIssue?.Args[0], resourceIssue?.Args[2]);
                break;
            default:
                throw new NotImplementedException($"Resource issue '{resourceIssue.Type}' is not implemented.");
        }
    }

    public bool TryGetOverride(ResourceId id, out RuleOverride? propertyOverride)
    {
        propertyOverride = null;
        return _Scope?.TryGetOverride(id, out propertyOverride) ?? false;
    }
}
