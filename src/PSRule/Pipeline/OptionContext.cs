// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    internal interface IBindingOption
    {
        FieldMap[] Field { get; }

        bool IgnoreCase { get; }

        string NameSeparator { get; }

        string[] TargetName { get; }

        string[] TargetType { get; }

        bool UseQualifiedName { get; }
    }

    internal sealed class OptionContext
    {
        private readonly Dictionary<string, BaselineScope> _ModuleBaselineScope;
        private readonly Dictionary<string, ConfigScope> _ModuleConfigScope;
        private readonly string[] _DefaultCulture;

        private BaselineScope _Parameter;
        private BaselineScope _Explicit;

        private BaselineScope _WorkspaceBaseline;
        private ConfigScope _WorkspaceConfig;

        private BaselineScope _ModuleBaseline;
        private ConfigScope _ModuleConfig;

        private BindingOption _Binding;
        private RuleFilter _Filter;
        private Dictionary<string, object> _Configuration;
        private string[] _Culture;

        internal OptionContext()
        {
            _ModuleBaselineScope = new Dictionary<string, BaselineScope>();
            _ModuleConfigScope = new Dictionary<string, ConfigScope>();
            _DefaultCulture = GetDefaultCulture();
        }

        internal enum ScopeType
        {
            Parameter = 0,
            Explicit = 1,
            Workspace = 2,
            Module = 3
        }

        internal abstract class OptionScope
        {
            public readonly ScopeType Type;
            public readonly string ModuleName;

            protected OptionScope(ScopeType type, string moduleName)
            {
                Type = type;
                ModuleName = moduleName;
            }
        }

        internal sealed class BaselineScope : OptionScope
        {
            public string Id;
            public bool Obsolete;

            // Rule
            public string[] Include;
            public string[] Exclude;
            public Hashtable Tag;

            // Configuration
            public Dictionary<string, object> Configuration;

            // Binding
            public FieldMap Field;
            public bool? IgnoreCase;
            public string NameSeparator;
            public string[] TargetName;
            public string[] TargetType;
            public bool? UseQualifiedName;

            public BaselineScope(ScopeType type, string baselineId, string moduleName, IBaselineSpec option, bool obsolete)
                : base(type, moduleName)
            {
                Id = baselineId;
                Obsolete = obsolete;
                Field = option.Binding?.Field;
                IgnoreCase = option.Binding?.IgnoreCase;
                NameSeparator = option?.Binding?.NameSeparator;
                TargetName = option.Binding?.TargetName;
                TargetType = option.Binding?.TargetType;
                UseQualifiedName = option.Binding?.UseQualifiedName;
                Include = option.Rule?.Include;
                Exclude = option.Rule?.Exclude;
                Tag = option.Rule?.Tag;
                Configuration = option.Configuration != null ?
                    new Dictionary<string, object>(option.Configuration, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public BaselineScope(ScopeType type, string[] include, Hashtable tag)
                : base(type, null)
            {
                Include = include;
                Tag = tag;
                Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        internal sealed class ConfigScope : OptionScope
        {
            // Configuration
            public Dictionary<string, object> Configuration;

            // Binding
            public FieldMap Field;
            public bool? IgnoreCase;
            public string NameSeparator;
            public string[] TargetName;
            public string[] TargetType;
            public bool? UseQualifiedName;

            // Output
            public string[] Culture;

            public ConfigScope(ScopeType type, string moduleName, PSRuleOption option)
                : base(type, moduleName)
            {
                Field = option.Binding?.Field;
                IgnoreCase = option.Binding?.IgnoreCase;
                NameSeparator = option?.Binding?.NameSeparator;
                TargetName = option.Binding?.TargetName;
                TargetType = option.Binding?.TargetType;
                UseQualifiedName = option.Binding?.UseQualifiedName;
                Culture = option.Output?.Culture;
                Configuration = option.Configuration != null ?
                    new Dictionary<string, object>(option.Configuration, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public ConfigScope(ScopeType type, string moduleName, ModuleConfigSpec spec)
                : base(type, moduleName)
            {
                Field = spec.Binding?.Field;
                IgnoreCase = spec.Binding?.IgnoreCase;
                NameSeparator = spec?.Binding?.NameSeparator;
                TargetName = spec.Binding?.TargetName;
                TargetType = spec.Binding?.TargetType;
                UseQualifiedName = spec.Binding?.UseQualifiedName;
                Culture = spec.Output?.Culture;
                Configuration = spec.Configuration != null ?
                    new Dictionary<string, object>(spec.Configuration, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private sealed class BindingOption : IBindingOption, IEquatable<BindingOption>
        {
            public BindingOption(FieldMap[] field, bool ignoreCase, string nameSeparator, string[] targetName, string[] targetType, bool useQualifiedName)
            {
                Field = field;
                IgnoreCase = ignoreCase;
                NameSeparator = nameSeparator;
                TargetName = targetName;
                TargetType = targetType;
                UseQualifiedName = useQualifiedName;
            }

            public FieldMap[] Field { get; }

            public bool IgnoreCase { get; }

            public string NameSeparator { get; }

            public string[] TargetName { get; }

            public string[] TargetType { get; }

            public bool UseQualifiedName { get; }

            public override bool Equals(object obj)
            {
                return obj is BindingOption option && Equals(option);
            }

            public bool Equals(BindingOption other)
            {
                return other != null &&
                    Field == other.Field &&
                    IgnoreCase == other.IgnoreCase &&
                    NameSeparator == other.NameSeparator &&
                    TargetName == other.TargetName &&
                    TargetType == other.TargetType &&
                    UseQualifiedName == other.UseQualifiedName;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine
                {
                    int hash = 17;
                    hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                    hash = hash * 23 + (IgnoreCase ? IgnoreCase.GetHashCode() : 0);
                    hash = hash * 23 + (NameSeparator != null ? NameSeparator.GetHashCode() : 0);
                    hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                    hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                    hash = hash * 23 + (UseQualifiedName ? UseQualifiedName.GetHashCode() : 0);
                    return hash;
                }
            }
        }

        public void UseScope(string moduleName)
        {
            _ModuleConfig = !string.IsNullOrEmpty(moduleName) && _ModuleConfigScope.TryGetValue(moduleName, out ConfigScope configScope) ? configScope : null;
            _ModuleBaseline = !string.IsNullOrEmpty(moduleName) && _ModuleBaselineScope.TryGetValue(moduleName, out BaselineScope baselineScope) ? baselineScope : null;
            _Binding = null;
            _Configuration = null;
            _Filter = null;
            _Culture = null;
        }

        public IResourceFilter RuleFilter()
        {
            if (_Filter != null)
                return _Filter;

            string[] include = _Parameter?.Include ?? _Explicit?.Include ?? _WorkspaceBaseline?.Include ?? _ModuleBaseline?.Include;
            string[] exclude = _Explicit?.Exclude ?? _WorkspaceBaseline?.Exclude ?? _ModuleBaseline?.Exclude;
            Hashtable tag = _Parameter?.Tag ?? _Explicit?.Tag ?? _WorkspaceBaseline?.Tag ?? _ModuleBaseline?.Tag;
            return _Filter = new RuleFilter(include, tag, exclude);
        }

        public IBindingOption GetTargetBinding()
        {
            if (_Binding != null)
                return _Binding;

            FieldMap[] field = new FieldMap[] { _Explicit?.Field, _WorkspaceBaseline?.Field, _ModuleBaseline?.Field, _ModuleConfig?.Field };
            bool ignoreCase = _Explicit?.IgnoreCase ?? _WorkspaceBaseline?.IgnoreCase ?? _ModuleBaseline?.IgnoreCase ?? _ModuleConfig?.IgnoreCase ?? Configuration.BindingOption.Default.IgnoreCase.Value;
            string nameSeparator = _Explicit?.NameSeparator ?? _WorkspaceBaseline?.NameSeparator ?? _ModuleBaseline?.NameSeparator ?? _ModuleConfig?.NameSeparator ?? Configuration.BindingOption.Default.NameSeparator;
            string[] targetName = _Explicit?.TargetName ?? _WorkspaceBaseline?.TargetName ?? _ModuleBaseline?.TargetName ?? _ModuleConfig?.TargetName;
            string[] targetType = _Explicit?.TargetType ?? _WorkspaceBaseline?.TargetType ?? _ModuleBaseline?.TargetType ?? _ModuleConfig?.TargetType;
            bool useQualifiedName = _Explicit?.UseQualifiedName ?? _WorkspaceBaseline?.UseQualifiedName ?? _ModuleBaseline?.UseQualifiedName ?? _ModuleConfig?.UseQualifiedName ?? Configuration.BindingOption.Default.UseQualifiedName.Value;
            return _Binding = new BindingOption(field, ignoreCase, nameSeparator, targetName, targetType, useQualifiedName);
        }

        public Dictionary<string, object> GetConfiguration()
        {
            if (_Configuration != null)
                return _Configuration;

            return _Configuration = AddConfiguration();
        }

        public string[] GetCulture()
        {
            if (_Culture != null)
                return _Culture;

            return _Culture = _WorkspaceConfig?.Culture ?? _ModuleConfig?.Culture ?? _DefaultCulture;
        }

        internal void Init(RunspaceContext context)
        {
            foreach (var baseline in _ModuleBaselineScope.Values)
            {
                if (baseline.Obsolete)
                    context.WarnBaselineObsolete(baseline.Id);
            }
            if (_Explicit != null && _Explicit.Obsolete)
                context.WarnBaselineObsolete(_Explicit.Id);
        }

        internal void Add(BaselineScope scope)
        {
            if (scope.Type == ScopeType.Module && !string.IsNullOrEmpty(scope.ModuleName))
                _ModuleBaselineScope.Add(scope.ModuleName, scope);
            else if (scope.Type == ScopeType.Explicit)
                _Explicit = scope;
            else if (scope.Type == ScopeType.Workspace)
                _WorkspaceBaseline = scope;
            else if (scope.Type == ScopeType.Parameter)
                _Parameter = scope;
        }

        internal void Add(ConfigScope scope)
        {
            if (scope.Type == ScopeType.Module && !string.IsNullOrEmpty(scope.ModuleName))
                _ModuleConfigScope.Add(scope.ModuleName, scope);
            else if (scope.Type == ScopeType.Workspace)
                _WorkspaceConfig = scope;
        }

        private Dictionary<string, object> AddConfiguration()
        {
            var result = new Dictionary<string, object>();
            if (_Explicit != null && _Explicit.Configuration.Count > 0)
                result.AddUnique(_Explicit.Configuration);

            if (_WorkspaceBaseline != null && _WorkspaceBaseline.Configuration.Count > 0)
                result.AddUnique(_WorkspaceBaseline.Configuration);

            if (_ModuleBaseline != null && _ModuleBaseline.Configuration.Count > 0)
                result.AddUnique(_ModuleBaseline.Configuration);

            if (_ModuleConfig != null && _ModuleConfig.Configuration.Count > 0)
                result.AddUnique(_ModuleConfig.Configuration);

            return result;
        }

        private static string[] GetDefaultCulture()
        {
            var result = new List<string>();
            var set = new HashSet<string>();

            // Fallback to current culture
            var current = PSRuleOption.GetCurrentCulture();
            if (!set.Contains(current.Name) && !string.IsNullOrEmpty(current.Name))
            {
                result.Add(current.Name);
                set.Add(current.Name);
            }
            for (var p = current.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
            {
                if (!result.Contains(p.Name))
                    result.Add(p.Name);
            }
            return result.ToArray();
        }
    }
}
