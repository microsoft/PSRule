// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// using System.Collections.Immutable;
using PSRule.Definitions;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A set of <see cref="ILanguageScope"/>.
/// </summary>
internal sealed class LanguageScopeSet : ILanguageScopeSet
{
    // private readonly ImmutableDictionary<string, ILanguageScope>? _Scopes;
    private readonly Dictionary<string, ILanguageScope> _Scopes = new(StringComparer.OrdinalIgnoreCase);

    private bool _Disposed;

    internal LanguageScopeSet() { }

    internal LanguageScopeSet(IDictionary<string, ILanguageScope> scopeSet)
    {
        if (scopeSet == null) throw new ArgumentNullException(nameof(scopeSet));

        // _Scopes = scopeSet.ToImmutableDictionary();
        foreach (var kv in scopeSet)
            Add(kv.Value);
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                // Release and dispose scopes
                if (_Scopes != null && _Scopes.Count > 0)
                {
                    foreach (var kv in _Scopes)
                        kv.Value.Dispose();

                    _Scopes.Clear();
                }
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    private void Add(ILanguageScope languageScope)
    {
        _Scopes.Add(languageScope.Name, languageScope);
    }

    /// <inheritdoc/>
    public IEnumerable<ILanguageScope> Get()
    {
        return _Scopes == null || _Scopes.Count == 0 ? [] : _Scopes.Values;
    }

    public bool TryScope(string? name, out ILanguageScope? scope)
    {
        scope = default;
        return _Scopes != null && _Scopes.TryGetValue(GetScopeName(name), out scope);
    }

    public bool Import(string name)
    {
        if (_Scopes.ContainsKey(GetScopeName(name)))
            return false;

        var scope = new LanguageScope(name);
        Add(scope);
        return true;
    }

    private static string GetScopeName(string? name)
    {
        return ResourceHelper.NormalizeScope(name);
    }
}

#nullable restore
