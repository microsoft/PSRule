// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline
{
    internal interface IBindingOption
    {
        FieldMap[] Field { get; }

        bool IgnoreCase { get; }

        string NameSeparator { get; }

        bool PreferTargetInfo { get; }

        string[] TargetName { get; }

        string[] TargetType { get; }

        bool UseQualifiedName { get; }
    }

    internal sealed class OptionContext
    {
        private readonly Dictionary<string, BaselineScope> _ModuleBaselineScope;
        private readonly Dictionary<string, ConfigScope> _ModuleConfigScope;
        private readonly List<string> _ConventionOrder;
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
        private ConventionFilter _ConventionFilter;

        internal OptionContext()
        {
            _ModuleBaselineScope = new Dictionary<string, BaselineScope>();
            _ModuleConfigScope = new Dictionary<string, ConfigScope>();
            _ConventionOrder = new List<string>();
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
            public bool? IncludeLocal;
            public string[] Include;
            public string[] Exclude;
            public Hashtable Tag;

            // Configuration
            public Dictionary<string, object> Configuration;

            public ConventionOption Convention;

            // Binding
            public FieldMap Field;
            public bool? IgnoreCase;
            public string NameSeparator;
            public bool? PreferTargetInfo;
            public string[] TargetName;
            public string[] TargetType;
            public bool? UseQualifiedName;

            public BaselineScope(ScopeType type, string baselineId, string moduleName, IBaselineV1Spec option, bool obsolete)
                : base(type, moduleName)
            {
                Id = baselineId;
                Obsolete = obsolete;
                Field = option.Binding?.Field;
                IgnoreCase = option.Binding?.IgnoreCase;
                NameSeparator = option?.Binding?.NameSeparator;
                PreferTargetInfo = option.Binding?.PreferTargetInfo;
                TargetName = option.Binding?.TargetName;
                TargetType = option.Binding?.TargetType;
                UseQualifiedName = option.Binding?.UseQualifiedName;
                IncludeLocal = option.Rule?.IncludeLocal;
                Include = option.Rule?.Include;
                Exclude = option.Rule?.Exclude;
                Tag = option.Rule?.Tag;
                Configuration = option.Configuration != null
                    ? new Dictionary<string, object>(option.Configuration, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                Convention = new ConventionOption(option.Convention);
            }

            public BaselineScope(ScopeType type, string[] include, Hashtable tag, string[] convention)
                : base(type, null)
            {
                Include = include;
                Tag = tag;
                Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                Convention = convention == null || convention.Length == 0 ? new ConventionOption() : new ConventionOption
                {
                    Include = convention
                };
            }
        }

        internal sealed class ConfigScope : OptionScope
        {
            // Configuration
            public Dictionary<string, object> Configuration;

            public ConventionOption Convention;

            // Binding
            public FieldMap Field;
            public bool? IgnoreCase;
            public string NameSeparator;
            public bool? PreferTargetInfo;
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
                PreferTargetInfo = option.Binding?.PreferTargetInfo;
                TargetName = option.Binding?.TargetName;
                TargetType = option.Binding?.TargetType;
                UseQualifiedName = option.Binding?.UseQualifiedName;
                Culture = option.Output?.Culture;
                Configuration = option.Configuration != null
                    ? new Dictionary<string, object>(option.Configuration, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                Convention = new ConventionOption(option.Convention);
            }

            public ConfigScope(ScopeType type, string moduleName, ModuleConfigV1Spec spec)
                : base(type, moduleName)
            {
                Field = spec.Binding?.Field;
                IgnoreCase = spec.Binding?.IgnoreCase;
                NameSeparator = spec?.Binding?.NameSeparator;
                PreferTargetInfo = spec.Binding?.PreferTargetInfo;
                TargetName = spec.Binding?.TargetName;
                TargetType = spec.Binding?.TargetType;
                UseQualifiedName = spec.Binding?.UseQualifiedName;
                Culture = spec.Output?.Culture;
                Configuration = spec.Configuration != null
                    ? new Dictionary<string, object>(spec.Configuration, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                Convention = new ConventionOption(spec.Convention);
            }
        }

        private sealed class BindingOption : IBindingOption, IEquatable<BindingOption>
        {
            public BindingOption(FieldMap[] field, bool ignoreCase, bool ignoreTargetInfo, string nameSeparator, string[] targetName, string[] targetType, bool useQualifiedName)
            {
                Field = field;
                IgnoreCase = ignoreCase;
                PreferTargetInfo = ignoreTargetInfo;
                NameSeparator = nameSeparator;
                TargetName = targetName;
                TargetType = targetType;
                UseQualifiedName = useQualifiedName;
            }

            public FieldMap[] Field { get; }

            public bool IgnoreCase { get; }

            public string NameSeparator { get; }

            public bool PreferTargetInfo { get; }

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
                    PreferTargetInfo == other.PreferTargetInfo &&
                    TargetName == other.TargetName &&
                    TargetType == other.TargetType &&
                    UseQualifiedName == other.UseQualifiedName;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine
                {
                    var hash = 17;
                    hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                    hash = hash * 23 + (IgnoreCase ? IgnoreCase.GetHashCode() : 0);
                    hash = hash * 23 + (NameSeparator != null ? NameSeparator.GetHashCode() : 0);
                    hash = hash * 23 + (PreferTargetInfo ? PreferTargetInfo.GetHashCode() : 0);
                    hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                    hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                    hash = hash * 23 + (UseQualifiedName ? UseQualifiedName.GetHashCode() : 0);
                    return hash;
                }
            }
        }

        public bool ContainsBaseline(string baselineId)
        {
            return _ModuleBaselineScope.ContainsKey(baselineId);
        }

        public void UseScope(string moduleName)
        {
            _ModuleConfig = !string.IsNullOrEmpty(moduleName) && _ModuleConfigScope.TryGetValue(moduleName, out var configScope) ? configScope : null;
            _ModuleBaseline = !string.IsNullOrEmpty(moduleName) && _ModuleBaselineScope.TryGetValue(moduleName, out var baselineScope) ? baselineScope : null;
            _Binding = null;
            _Configuration = null;
            _Filter = null;
            _Culture = null;
            _ConventionFilter = null;
        }

        private IResourceFilter GetRuleFilter()
        {
            if (_Filter != null)
                return _Filter;

            var include = _Parameter?.Include ?? _Explicit?.Include ?? _WorkspaceBaseline?.Include ?? _ModuleBaseline?.Include;
            var exclude = _Explicit?.Exclude ?? _WorkspaceBaseline?.Exclude ?? _ModuleBaseline?.Exclude;
            var tag = _Parameter?.Tag ?? _Explicit?.Tag ?? _WorkspaceBaseline?.Tag ?? _ModuleBaseline?.Tag;
            var includeLocal = _Explicit?.IncludeLocal ?? _WorkspaceBaseline?.IncludeLocal ?? _ModuleBaseline?.IncludeLocal;
            return _Filter = new RuleFilter(include, tag, exclude, includeLocal);
        }

        private IResourceFilter GetConventionFilter()
        {
            if (_ConventionFilter != null)
                return _ConventionFilter;

            var include = new List<string>();
            for (var i = 0; _Parameter?.Convention?.Include != null && i < _Parameter.Convention.Include.Length; i++)
                include.Add(_Parameter.Convention.Include[i]);

            for (var i = 0; _WorkspaceConfig?.Convention?.Include != null && i < _WorkspaceConfig.Convention.Include.Length; i++)
                include.Add(_WorkspaceConfig.Convention.Include[i]);

            for (var i = 0; _ModuleConfig?.Convention?.Include != null && i < _ModuleConfig.Convention.Include.Length; i++)
                include.Add(ResourceHelper.GetIdString(_ModuleConfig.ModuleName, _ModuleConfig.Convention.Include[i]));

            return _ConventionFilter = new ConventionFilter(include.ToArray());
        }

        public IBindingOption GetTargetBinding()
        {
            if (_Binding != null)
                return _Binding;

            var field = new FieldMap[] { _Explicit?.Field, _WorkspaceBaseline?.Field, _ModuleBaseline?.Field, _ModuleConfig?.Field };
            var ignoreCase = _Explicit?.IgnoreCase ?? _WorkspaceBaseline?.IgnoreCase ?? _ModuleBaseline?.IgnoreCase ?? _ModuleConfig?.IgnoreCase ?? Configuration.BindingOption.Default.IgnoreCase.Value;
            var nameSeparator = _Explicit?.NameSeparator ?? _WorkspaceBaseline?.NameSeparator ?? _ModuleBaseline?.NameSeparator ?? _ModuleConfig?.NameSeparator ?? Configuration.BindingOption.Default.NameSeparator;
            var preferTargetInfo = _Explicit?.PreferTargetInfo ?? _WorkspaceBaseline?.PreferTargetInfo ?? _ModuleBaseline?.PreferTargetInfo ?? _ModuleConfig?.PreferTargetInfo ?? Configuration.BindingOption.Default.PreferTargetInfo.Value;
            var targetName = _Explicit?.TargetName ?? _WorkspaceBaseline?.TargetName ?? _ModuleBaseline?.TargetName ?? _ModuleConfig?.TargetName;
            var targetType = _Explicit?.TargetType ?? _WorkspaceBaseline?.TargetType ?? _ModuleBaseline?.TargetType ?? _ModuleConfig?.TargetType;
            var useQualifiedName = _Explicit?.UseQualifiedName ?? _WorkspaceBaseline?.UseQualifiedName ?? _ModuleBaseline?.UseQualifiedName ?? _ModuleConfig?.UseQualifiedName ?? Configuration.BindingOption.Default.UseQualifiedName.Value;
            return _Binding = new BindingOption(field, ignoreCase, preferTargetInfo, nameSeparator, targetName, targetType, useQualifiedName);
        }

        public Dictionary<string, object> GetConfiguration()
        {
            return _Configuration ??= AddConfiguration();
        }

        public string[] GetCulture()
        {
            return _Culture ??= _WorkspaceConfig?.Culture ?? _ModuleConfig?.Culture ?? _DefaultCulture;
        }

        internal void Init(RunspaceContext context)
        {
            foreach (var baseline in _ModuleBaselineScope.Values)
            {
                if (baseline.Obsolete)
                    context.WarnResourceObsolete(ResourceKind.Baseline, baseline.Id);
            }
            if (_Explicit != null && _Explicit.Obsolete)
                context.WarnResourceObsolete(ResourceKind.Baseline, _Explicit.Id);
        }

        internal void Add(BaselineScope scope)
        {
            if (scope == null)
                return;

            var conventions = scope.Convention?.Include;
            if (scope.Type == ScopeType.Module && !string.IsNullOrEmpty(scope.ModuleName) && !_ModuleBaselineScope.ContainsKey(scope.ModuleName))
            {
                _ModuleBaselineScope.Add(scope.ModuleName, scope);
                conventions = GetConventions(scope.ModuleName, scope.Convention?.Include);
            }
            else if (scope.Type == ScopeType.Explicit)
            {
                _Explicit = scope;
                if (scope.ModuleName != null)
                    conventions = GetConventions(scope.ModuleName, scope.Convention?.Include);
            }
            else if (scope.Type == ScopeType.Workspace)
                _WorkspaceBaseline = scope;
            else if (scope.Type == ScopeType.Parameter)
                _Parameter = scope;

            if (conventions != null)
                _ConventionOrder.AddRange(conventions);
        }

        internal void Add(ConfigScope scope)
        {
            if (scope == null)
                return;

            var conventions = scope.Convention?.Include;
            if (scope.Type == ScopeType.Module && !string.IsNullOrEmpty(scope.ModuleName))
            {
                _ModuleConfigScope.Add(scope.ModuleName, scope);
                conventions = GetConventions(scope.ModuleName, scope.Convention?.Include);
            }
            else if (scope.Type == ScopeType.Workspace)
                _WorkspaceConfig = scope;

            if (conventions != null)
                _ConventionOrder.AddRange(conventions);
        }

        internal int GetConventionOrder(IConvention convention)
        {
            var index = _ConventionOrder.IndexOf(convention.Id.Value);
            if (index == -1)
                index = _ConventionOrder.IndexOf(convention.Name);

            return index > -1 ? index : int.MaxValue;
        }

        private static string[] GetConventions(string scope, string[] include)
        {
            if (include == null || include.Length == 0)
                return null;

            for (var i = 0; i < include.Length; i++)
                include[i] = ResourceHelper.GetIdString(scope, include[i]);

            return include;
        }

        internal void BuildScope(ILanguageScope languageScope)
        {
            UseScope(languageScope.Name);
            var configuration = GetConfiguration();
            languageScope.Configure(configuration);
            languageScope.WithFilter(GetRuleFilter());
            languageScope.WithFilter(GetConventionFilter());
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

    internal sealed class OptionContextBuilder
    {
        private readonly OptionContext _OptionContext;

        internal OptionContextBuilder()
        {
            _OptionContext = new OptionContext();
        }

        internal OptionContextBuilder(PSRuleOption option, string[] include, Hashtable tag, string[] convention)
            : this()
        {
            Parameter(include, tag, convention);
            Workspace(option);
        }

        internal OptionContext Build()
        {
            return _OptionContext;
        }

        private void Parameter(string[] include, Hashtable tag, string[] convention)
        {
            _OptionContext.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Parameter,
                include: include,
                tag: tag,
                convention: convention));
        }

        private void Workspace(PSRuleOption option)
        {
            _OptionContext.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Workspace,
                baselineId: null,
                moduleName: null,
                option: option,
                obsolete: false));

            _OptionContext.Add(new OptionContext.ConfigScope(
                type: OptionContext.ScopeType.Workspace,
                moduleName: null,
                option: option));
        }
    }
}
