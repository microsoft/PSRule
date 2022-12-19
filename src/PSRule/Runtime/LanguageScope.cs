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
        /// <summary>
        /// The name of the scope.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Adds one or more configuration values to the scope.
        /// </summary>
        void Configure(Dictionary<string, object> configuration);

        /// <summary>
        /// Try to get a specific configuration value by name.
        /// </summary>
        bool TryConfigurationValue(string key, out object value);

        void WithFilter(IResourceFilter resourceFilter);

        IResourceFilter GetFilter(ResourceKind kind);

        /// <summary>
        /// Add a service to the scope.
        /// </summary>
        void AddService(string name, object service);

        /// <summary>
        /// Get a previously added service.
        /// </summary>
        object GetService(string name);

        bool TryGetType(object o, out string type, out string path);

        bool TryGetName(object o, out string name, out string path);

        bool TryGetScope(object o, out string[] scope);
    }

    internal sealed class LanguageScope : ILanguageScope
    {
        private const string STANDALONE_SCOPENAME = ".";

        private readonly RunspaceContext _Context;
        private readonly Dictionary<string, object> _Configuration;
        private readonly Dictionary<string, object> _Service;
        private readonly Dictionary<ResourceKind, IResourceFilter> _Filter;

        private bool _Disposed;

        public LanguageScope(RunspaceContext context, string name)
        {
            _Context = context;
            Name = Normalize(name);
            _Configuration = new Dictionary<string, object>();
            _Filter = new Dictionary<ResourceKind, IResourceFilter>();
            _Service = new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public string Name { get; }

        public void Configure(Dictionary<string, object> configuration)
        {
            _Configuration.AddUnique(configuration);
        }

        public bool TryConfigurationValue(string key, out object value)
        {
            value = null;
            return !string.IsNullOrEmpty(key) && _Configuration.TryGetValue(key, out value);
        }

        public void WithFilter(IResourceFilter resourceFilter)
        {
            _Filter[resourceFilter.Kind] = resourceFilter;
        }

        public IResourceFilter GetFilter(ResourceKind kind)
        {
            return _Filter.TryGetValue(kind, out var filter) ? filter : null;
        }

        /// <inheritdoc/>
        public void AddService(string name, object service)
        {
            if (_Service.ContainsKey(name))
                return;

            _Service.Add(name, service);
        }

        /// <inheritdoc/>
        public object GetService(string name)
        {
            return _Service.TryGetValue(name, out var service) ? service : null;
        }

        public bool TryGetType(object o, out string type, out string path)
        {
            if (_Context != null && _Context.TargetObject.Value == o)
            {
                var binding = _Context.TargetBinder.Result(Name);
                type = binding.TargetType;
                path = binding.TargetTypePath;
                return true;
            }
            else if (_Context != null)
            {
                var binding = _Context.TargetBinder.Using(Name).Bind(o);
                type = binding.TargetType;
                path = binding.TargetTypePath;
                return true;
            }
            type = null;
            path = null;
            return false;
        }

        public bool TryGetName(object o, out string name, out string path)
        {
            if (_Context != null && _Context.TargetObject.Value == o)
            {
                var binding = _Context.TargetBinder.Result(Name);
                name = binding.TargetName;
                path = binding.TargetNamePath;
                return true;
            }
            else if (_Context != null)
            {
                var binding = _Context.TargetBinder.Using(Name).Bind(o);
                name = binding.TargetName;
                path = binding.TargetNamePath;
                return true;
            }
            name = null;
            path = null;
            return false;
        }

        public bool TryGetScope(object o, out string[] scope)
        {
            if (_Context != null && _Context.TargetObject.Value == o)
            {
                scope = _Context.TargetObject.Scope;
                return true;
            }
            scope = null;
            return false;
        }

        internal static string Normalize(string scope)
        {
            return string.IsNullOrEmpty(scope) ? STANDALONE_SCOPENAME : scope;
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
        private readonly RunspaceContext _Context;
        private readonly Dictionary<string, ILanguageScope> _Scopes;

        private ILanguageScope _Current;
        private bool _Disposed;

        public LanguageScopeSet(RunspaceContext context)
        {
            _Context = context;
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
            if (_Scopes.TryGetValue(GetScopeName(name), out var scope))
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

            scope = new LanguageScope(_Context, name);
            Add(scope);
            return true;
        }

        private static string GetScopeName(string name)
        {
            return LanguageScope.Normalize(name);
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
