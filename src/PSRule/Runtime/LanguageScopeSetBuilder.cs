// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;

namespace PSRule.Runtime;

#nullable enable

internal sealed class LanguageScopeSetBuilder
{
    private readonly Dictionary<string, ILanguageScope> _Scopes;
    private readonly RuntimeFactoryBuilder _RuntimeFactoryBuilder;

    public LanguageScopeSetBuilder()
    {
        _Scopes = new Dictionary<string, ILanguageScope>(StringComparer.OrdinalIgnoreCase)
        {
            { ResourceHelper.NormalizeScope(null), new LanguageScope(ResourceHelper.NormalizeScope(null), null) }
        };
        _RuntimeFactoryBuilder = new RuntimeFactoryBuilder(null);
    }

    /// <summary>
    /// Perform initialization of the builder from the environment.
    /// </summary>
    public void Init(Source[]? sources)
    {
        // Create all the module scopes from known sources.
        Import(sources);
    }

    /// <summary>
    /// Create a scope for a module.
    /// </summary>
    /// <param name="name">The name of the module. This must be a unique non-empty string.</param>
    /// <param name="module">Information about the module.</param>
    public void CreateModuleScope(string name, Source.ModuleInfo? module)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        if (_Scopes.ContainsKey(name)) throw new ArgumentException(nameof(name));

        var container = module == null || module.Assemblies == null || module.Assemblies.Length == 0 ? null : _RuntimeFactoryBuilder.BuildFromAssembly(name, module.Assemblies);
        var scope = new LanguageScope(name, container);

        _Scopes.Add(name, scope);
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
                CreateModuleScope(source.Scope, source.Module);
                moduleNames.Add(source.Scope);
            }
        }
    }
}

#nullable restore
