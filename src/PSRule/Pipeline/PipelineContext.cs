// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Host;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

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
        private readonly IList<ResourceRef> _Unresolved;
        private readonly LanguageMode _LanguageMode;
        private readonly Dictionary<string, PathExpression> _PathExpressionCache;
        private readonly List<ResourceIssue> _TrackedIssues;

        // Objects kept for caching and disposal
        private Runspace _Runspace;
        private SHA1Managed _Hash;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal PSRuleOption Option;

        internal readonly Dictionary<string, Hashtable> LocalizedDataCache;
        internal readonly Dictionary<string, object> ExpressionCache;
        internal readonly Dictionary<string, PSObject[]> ContentCache;
        internal readonly Dictionary<string, SelectorVisitor> Selector;
        internal readonly List<SuppressionGroupVisitor> SuppressionGroup;
        internal readonly OptionContext Baseline;
        internal readonly HostContext HostContext;
        internal readonly PipelineReader Reader;
        internal readonly BindTargetMethod BindTargetName;
        internal readonly BindTargetMethod BindTargetType;
        internal readonly BindTargetMethod BindField;
        internal readonly string RunId;

        internal readonly Stopwatch RunTime;

        public HashAlgorithm ObjectHashAlgorithm
        {
            get
            {
                if (_Hash == null)
                    _Hash = new SHA1Managed();

                return _Hash;
            }
        }

        private PipelineContext(PSRuleOption option, HostContext hostContext, PipelineReader reader, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, OptionContext baseline, IList<ResourceRef> unresolved)
        {
            Option = option;
            HostContext = hostContext;
            Reader = reader;
            BindTargetName = bindTargetName;
            BindTargetType = bindTargetType;
            BindField = bindField;
            _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode.Value;
            _PathExpressionCache = new Dictionary<string, PathExpression>();
            LocalizedDataCache = new Dictionary<string, Hashtable>();
            ExpressionCache = new Dictionary<string, object>();
            ContentCache = new Dictionary<string, PSObject[]>();
            Selector = new Dictionary<string, SelectorVisitor>();
            SuppressionGroup = new List<SuppressionGroupVisitor>();
            Baseline = baseline;
            _Unresolved = unresolved ?? new List<ResourceRef>();
            _TrackedIssues = new List<ResourceIssue>();
            RunId = EnvironmentHelper.Default.GetRunId() ?? ObjectHashAlgorithm.GetDigest(Guid.NewGuid().ToByteArray());
            RunTime = Stopwatch.StartNew();
        }

        public static PipelineContext New(PSRuleOption option, HostContext hostContext, PipelineReader reader, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, OptionContext baseline, IList<ResourceRef> unresolved)
        {
            var context = new PipelineContext(option, hostContext, reader, bindTargetName, bindTargetType, bindField, baseline, unresolved);
            CurrentThread = context;
            return context;
        }

        internal sealed class SourceScope
        {
            public readonly SourceFile File;
            public readonly string[] SourceContentCache;

            public SourceScope(SourceFile source, string[] content)
            {
                File = source;
                SourceContentCache = content;
            }
        }

        internal enum ResourceIssueType
        {
            Unknown
        }

        internal sealed class ResourceIssue
        {
            public ResourceIssue(ResourceKind kind, string id, ResourceIssueType issue)
            {
                Kind = kind;
                Id = id;
                Issue = issue;
            }

            public ResourceKind Kind { get; }

            public string Id { get; }

            public ResourceIssueType Issue { get; }
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
            TrackIssue(resource);
            if (TryBaseline(resource, out var baseline) && TryBaselineRef(resource.Id, out var baselineRef))
            {
                _Unresolved.Remove(baselineRef);
                Baseline.Add(new OptionContext.BaselineScope(baselineRef.Type, baseline.BaselineId, resource.Source.Module, baseline.Spec, baseline.Obsolete));
            }
            else if (resource.Kind == ResourceKind.Selector && resource is SelectorV1 selector)
                Selector[selector.Id.Value] = new SelectorVisitor(selector.Id, selector.Source, selector.Spec.If);
            else if (TryModuleConfig(resource, out var moduleConfig))
            {
                if (!string.IsNullOrEmpty(moduleConfig?.Spec?.Rule?.Baseline))
                {
                    var baselineId = ResourceHelper.GetIdString(moduleConfig.Source.Module, moduleConfig.Spec.Rule.Baseline);
                    if (!Baseline.ContainsBaseline(baselineId))
                        _Unresolved.Add(new BaselineRef(baselineId, OptionContext.ScopeType.Module));
                }
                Baseline.Add(new OptionContext.ConfigScope(OptionContext.ScopeType.Module, resource.Source.Module, moduleConfig?.Spec));
            }
            else if (resource.Kind == ResourceKind.SuppressionGroup && resource is SuppressionGroupV1 suppressionGroup)
            {
                var suppressionGroupVisitor = new SuppressionGroupVisitor(
                    id: suppressionGroup.Id,
                    source: suppressionGroup.Source,
                    spec: suppressionGroup.Spec
                );
                SuppressionGroup.Add(suppressionGroupVisitor);
            }
        }

        private void TrackIssue(IResource resource)
        {
            //if (resource.TryValidateResourceAnnotation())
            //    _TrackedIssues.Add(new ResourceIssue(resource.Kind, resource.Id, ResourceIssueType.MissingApiVersion));
        }

        private bool TryBaselineRef(ResourceId resourceId, out BaselineRef baselineRef)
        {
            baselineRef = null;
            var r = _Unresolved.FirstOrDefault(i => ResourceIdEqualityComparer.IdEquals(i.Id, resourceId.Value));
            if (!(r is BaselineRef br))
                return false;

            baselineRef = br;
            return true;
        }

        private static bool TryBaseline(IResource resource, out Baseline baseline)
        {
            baseline = null;
            if (resource.Kind == ResourceKind.Baseline && resource is Baseline result)
            {
                baseline = result;
                return true;
            }
            return false;
        }

        private static bool TryModuleConfig(IResource resource, out ModuleConfigV1 moduleConfig)
        {
            moduleConfig = null;
            if (resource.Kind == ResourceKind.ModuleConfig &&
                !string.IsNullOrEmpty(resource.Source.Module) &&
                StringComparer.OrdinalIgnoreCase.Equals(resource.Source.Module, resource.Name) &&
                resource is ModuleConfigV1 result)
            {
                moduleConfig = result;
                return true;
            }
            return false;
        }

        internal void Begin(RunspaceContext runspaceContext)
        {
            ReportIssue(runspaceContext);
            Baseline.Init(runspaceContext);
        }

        /// <summary>
        /// Report any tracked issues.
        /// </summary>
        private void ReportIssue(RunspaceContext runspaceContext)
        {
            //for (var i = 0; _TrackedIssues != null && i < _TrackedIssues.Count; i++)
            //if (_TrackedIssues[i].Issue == ResourceIssueType.MissingApiVersion)
            //    runspaceContext.WarnMissingApiVersion(_TrackedIssues[i].Kind, _TrackedIssues[i].Id);
        }

        #region IBindingContext

        public bool GetPathExpression(string path, out PathExpression expression)
        {
            return _PathExpressionCache.TryGetValue(path, out expression);
        }

        public void CachePathExpression(string path, PathExpression expression)
        {
            _PathExpressionCache[path] = expression;
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

                    _PathExpressionCache.Clear();
                    LocalizedDataCache.Clear();
                    ExpressionCache.Clear();
                    ContentCache.Clear();
                    RunTime.Stop();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
