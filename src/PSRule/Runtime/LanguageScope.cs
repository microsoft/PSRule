// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System;
using System.Collections.Generic;

namespace PSRule.Runtime
{
    /// <summary>
    /// A named scope for langauge elements.
    /// </summary>
    internal interface ILanguageScope
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
    }

    internal sealed class LanguageScope : ILanguageScope
    {
        internal const string STANDALONE_SCOPENAME = ".";

        private readonly Dictionary<string, object> _Configuration;

        private Dictionary<ResourceKind, IResourceFilter> _Filter;

        public LanguageScope(string name)
        {
            Name = name ?? STANDALONE_SCOPENAME;
            _Configuration = new Dictionary<string, object>();
            _Filter = new Dictionary<ResourceKind, IResourceFilter>();
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
    }

    internal sealed class LanguageScopeSet
    {
        private readonly Dictionary<string, ILanguageScope> _Scopes;

        private ILanguageScope _Current;

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
    }
}
