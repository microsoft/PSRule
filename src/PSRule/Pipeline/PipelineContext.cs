// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Rules;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext : IDisposable, IBindingContext
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";

        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        // Configuration parameters
        internal readonly TargetBinder Binder;
        private readonly IDictionary<string, ResourceRef> _Unresolved;
        private readonly LanguageMode _LanguageMode;
        private readonly Dictionary<string, NameToken> _NameTokenCache;

        // Objects kept for caching and disposal
        private Runspace _Runspace;
        private SHA1Managed _Hash;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal PSRuleOption Option;

        internal ExecutionScope ExecutionScope;

        internal readonly Dictionary<string, Hashtable> LocalizedDataCache;
        internal readonly Dictionary<string, object> ExpressionCache;
        internal readonly Dictionary<string, PSObject[]> ContentCache;
        internal readonly OptionContext Baseline;
        internal readonly HostContext HostContext;

        public HashAlgorithm ObjectHashAlgorithm
        {
            get
            {
                if (_Hash == null)
                    _Hash = new SHA1Managed();

                return _Hash;
            }
        }

        private PipelineContext(PSRuleOption option, HostContext hostContext, TargetBinder binder, OptionContext baseline, IDictionary<string, ResourceRef> unresolved)
        {
            Option = option;
            HostContext = hostContext;
            _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode.Value;
            _NameTokenCache = new Dictionary<string, NameToken>();
            LocalizedDataCache = new Dictionary<string, Hashtable>();
            ExpressionCache = new Dictionary<string, object>();
            ContentCache = new Dictionary<string, PSObject[]>();
            Binder = binder;
            Baseline = baseline;
            _Unresolved = unresolved;
        }

        public static PipelineContext New(PSRuleOption option, HostContext hostContext, TargetBinder binder, OptionContext baseline, IDictionary<string, ResourceRef> unresolved)
        {
            var context = new PipelineContext(option, hostContext, binder, baseline, unresolved);
            CurrentThread = context;
            return context;
        }

        internal sealed class SourceScope
        {
            public readonly SourceFile File;
            public readonly string[] SourceContentCache;
            public readonly IResourceFilter Filter;
            public readonly Dictionary<string, object> Configuration;

            public SourceScope(SourceFile source, string[] content, IResourceFilter filter, Dictionary<string, object> configuration)
            {
                File = source;
                SourceContentCache = content;
                Filter = filter;
                Configuration = configuration;
            }
        }

        internal Runspace GetRunspace()
        {
            if (_Runspace == null)
            {
                var state = HostState.CreateSessionState();
                state.LanguageMode = _LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

                _Runspace = RunspaceFactory.CreateRunspace(state);
                if (Runspace.DefaultRunspace == null)
                    Runspace.DefaultRunspace = _Runspace;

                _Runspace.Open();
                _Runspace.SessionStateProxy.PSVariable.Set(new PSRuleVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new RuleVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new LocalizedDataVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new AssertVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new ConfigurationVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(ErrorPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(WarningPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(VerbosePreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(DebugPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.Path.SetLocation(PSRuleOption.GetWorkingPath());
            }
            return _Runspace;
        }

        internal void Import(IResource resource)
        {
            if (resource.Kind == ResourceKind.Baseline && resource is Baseline baseline && _Unresolved.TryGetValue(resource.Id, out ResourceRef rr) && rr is BaselineRef baselineRef)
            {
                _Unresolved.Remove(resource.Id);
                Baseline.Add(new OptionContext.BaselineScope(baselineRef.Type, baseline.BaselineId, resource.Module, baseline.Spec, baseline.Obsolete));
            }
            else if (TryModuleConfig(resource, out ModuleConfig moduleConfig))
            {
                Baseline.Add(new OptionContext.ConfigScope(OptionContext.ScopeType.Module, resource.Module, moduleConfig.Spec));
            }
        }

        private static bool TryModuleConfig(IResource resource, out ModuleConfig moduleConfig)
        {
            moduleConfig = null;
            if (resource.Kind == ResourceKind.ModuleConfig && !string.IsNullOrEmpty(resource.Module) && resource.Module == resource.Name && resource is ModuleConfig result)
            {
                moduleConfig = result;
                return true;
            }
            return false;
        }

        public bool ShouldFilter()
        {
            return Binder.ShouldFilter;
        }

        #region IBindingContext

        public bool GetNameToken(string expression, out NameToken nameToken)
        {
            if (!_NameTokenCache.ContainsKey(expression))
            {
                nameToken = null;
                return false;
            }
            nameToken = _NameTokenCache[expression];
            return true;
        }

        public void CacheNameToken(string expression, NameToken nameToken)
        {
            _NameTokenCache[expression] = nameToken;
        }

        #endregion IBindingContext

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    if (_Hash != null)
                        _Hash.Dispose();

                    if (_Runspace != null)
                        _Runspace.Dispose();

                    _NameTokenCache.Clear();
                    LocalizedDataCache.Clear();
                    ExpressionCache.Clear();
                    ContentCache.Clear();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
