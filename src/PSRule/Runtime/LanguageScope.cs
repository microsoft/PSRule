// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSRule.Definitions;

namespace PSRule.Runtime
{
    /// <summary>
    /// A named scope for langauge elements.
    /// </summary>
    internal interface ILanguageScope : IDisposable
    {
        string Name { get; }

        /// <summary>
        /// Adds one or more configuration values to the scope.
        /// </summary>
        void Configure(Dictionary<string, object> configuration);

        /// <summary>
        /// Try to get a specific configuration value by name.
        /// </summary>
        bool TryConfigurationValue(string name, out object result);

        void WithFilter(IResourceFilter resourceFilter);

        IResourceFilter GetFilter(ResourceKind kind);

        void AddService(string name, object service);

        object GetService(string name);
    }

    internal sealed class LanguageScope : ILanguageScope
    {
        internal const string STANDALONE_SCOPENAME = ".";

        private readonly Dictionary<string, object> _Configuration;
        private readonly Dictionary<string, object> _Service;

        private readonly Dictionary<ResourceKind, IResourceFilter> _Filter;
        private bool _Disposed;

        public LanguageScope(string name)
        {
            Name = name ?? STANDALONE_SCOPENAME;
            _Configuration = new Dictionary<string, object>();
            _Filter = new Dictionary<ResourceKind, IResourceFilter>();
            _Service = new Dictionary<string, object>();
        }

        public string Name { get; }

        public void Configure(Dictionary<string, object> configuration)
        {
            _Configuration.AddUnique(configuration);
        }

        public bool TryConfigurationValue(string key, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(key))
                return false;

            return _Configuration.TryGetValue(key, out value);
        }

        public void WithFilter(IResourceFilter resourceFilter)
        {
            _Filter[resourceFilter.Kind] = resourceFilter;
        }

        public IResourceFilter GetFilter(ResourceKind kind)
        {
            return _Filter.TryGetValue(kind, out IResourceFilter filter) ? filter : null;
        }

        public void AddService(string name, object service)
        {
            if (_Service.ContainsKey(name))
                return;

            _Service.Add(name, service);
        }

        public object GetService(string name)
        {
            return _Service.TryGetValue(name, out object service) ? service : null;
        }

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    // Release and dispose services
                    if (_Service != null && _Service.Count > 0)
                    {
                        foreach (var kv in _Service)
                        {
                            if (kv.Value is IDisposable d)
                                d.Dispose();
                        }
                        _Service.Clear();
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
    }

    internal sealed class LanguageScopeSet : IDisposable
    {
        private readonly Dictionary<string, ILanguageScope> _Scopes;

        private ILanguageScope _Current;
        private bool _Disposed;

        public LanguageScopeSet()
        {
            _Scopes = new Dictionary<string, ILanguageScope>(StringComparer.OrdinalIgnoreCase);
            Import(null, out _Current);
        }

        public ILanguageScope Current
        {
            get
            {
                return _Current;
            }
        }

        internal void Add(ILanguageScope languageScope)
        {
            _Scopes.Add(languageScope.Name, languageScope);
        }

        internal IEnumerable<ILanguageScope> Get()
        {
            return _Scopes.Values;
        }

        internal void UseScope(string name)
        {
            if (_Scopes.TryGetValue(GetScopeName(name), out ILanguageScope scope))
                _Current = scope;
        }

        internal bool TryScope(string name, out ILanguageScope scope)
        {
            return _Scopes.TryGetValue(GetScopeName(name), out scope);
        }

        internal bool Import(string name, out ILanguageScope scope)
        {
            if (_Scopes.TryGetValue(GetScopeName(name), out scope))
                return false;

            scope = new LanguageScope(name);
            Add(scope);
            return true;
        }

        private static string GetScopeName(string name)
        {
            return name ?? LanguageScope.STANDALONE_SCOPENAME;
        }

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
    }
}
