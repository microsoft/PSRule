// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;

namespace PSRule.Runtime;

#nullable enable

internal sealed class LanguageScopeSetBuilder
{
    private readonly Dictionary<string, ILanguageScope> _Scopes;

    public LanguageScopeSetBuilder()
    {
        _Scopes = new Dictionary<string, ILanguageScope>(StringComparer.OrdinalIgnoreCase)
        {
            { ResourceHelper.NormalizeScope(null), new LanguageScope(ResourceHelper.NormalizeScope(null)) }
        };
    }

    /// <summary>
    /// Perform initialization of the builder from the environment.
    /// </summary>
    public void Init(PSRuleOption? option, Source[]? sources)
    {
        // Create all the module scopes from known sources.
        Import(sources);

        // Use options to configure the root scope.
        Configure(option);
    }

    /// <summary>
    /// Create a scope for a module.
    /// </summary>
    /// <param name="name">The name of the module. This must be a unique non-empty string.</param>
    public void CreateModuleScope(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        if (_Scopes.ContainsKey(name)) throw new ArgumentException(nameof(name));

        _Scopes.Add(name, new LanguageScope(name));
    }

    /// <summary>
    /// Build the collection.
    /// </summary>
    /// <returns>Returns a collection of language scopes.</returns>
    public ILanguageScopeSet Build()
    {
        return new LanguageScopeSet(_Scopes);
    }

    /// <summary>
    /// Configure the root scope with options.
    /// </summary>
    /// <param name="option"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void Configure(PSRuleOption? option)
    {
        // Do nothing currently.
    }

    /// <summary>
    /// Import modules scopes from sources.
    /// </summary>
    private void Import(Source[]? sources)
    {
        if (sources == null || sources.Length == 0)
            return;

        // Create all the module scopes from known sources.
        var moduleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in sources.Where(s => s.Scope != "." && !string.IsNullOrEmpty(s.Scope)))
        {
            if (!moduleNames.Contains(source.Scope))
            {
                CreateModuleScope(source.Scope);
                moduleNames.Add(source.Scope);
            }
        }
    }
}

#nullable restore
