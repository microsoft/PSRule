// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Runtime;

/// <summary>
/// A collection of <see cref="ILanguageScope"/>.
/// </summary>
internal sealed class LanguageScopeSet : ILanguageScopeCollection
{
    private readonly Dictionary<string, ILanguageScope> _Scopes;

    private ILanguageScope _Current;
    private bool _Disposed;

    public LanguageScopeSet()
    {
        _Scopes = new Dictionary<string, ILanguageScope>(StringComparer.OrdinalIgnoreCase);
        Import(null);
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

    internal void Add(ILanguageScope languageScope)
    {
        _Scopes.Add(languageScope.Name, languageScope);
    }

    public IEnumerable<ILanguageScope> Get()
    {
        return _Scopes.Values;
    }

    public bool TryScope(string name, out ILanguageScope scope)
    {
        return _Scopes.TryGetValue(GetScopeName(name), out scope);
    }

    public bool Import(string name)
    {
        if (_Scopes.ContainsKey(GetScopeName(name)))
            return false;

        var scope = new LanguageScope(name);
        Add(scope);
        return true;
    }

    private static string GetScopeName(string name)
    {
        return ResourceHelper.NormalizeScope(name);
    }
}
