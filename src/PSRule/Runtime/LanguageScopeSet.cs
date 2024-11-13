// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using PSRule.Definitions;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A set of <see cref="ILanguageScope"/>.
/// </summary>
internal sealed class LanguageScopeSet : ILanguageScopeSet
{
    private readonly ImmutableDictionary<string, ILanguageScope>? _Scopes;

    private bool _Disposed;

    internal LanguageScopeSet(IDictionary<string, ILanguageScope> scopeSet)
    {
        if (scopeSet == null) throw new ArgumentNullException(nameof(scopeSet));

        _Scopes = scopeSet.ToImmutableDictionary();
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
                    {
                        kv.Value.Dispose();
                    }
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

    private static string GetScopeName(string? name)
    {
        return ResourceHelper.NormalizeScope(name);
    }
}

#nullable restore
