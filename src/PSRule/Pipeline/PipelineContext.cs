// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Host;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Pipeline;

internal sealed class PipelineContext : IDisposable, IBindingContext
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string DebugPreference = "DebugPreference";

    [ThreadStatic]
    internal static PipelineContext CurrentThread;

    private readonly OptionContextBuilder _OptionBuilder;

    // Configuration parameters
    private readonly IList<ResourceRef> _Unresolved;
    private readonly LanguageMode _LanguageMode;
    private readonly Dictionary<string, PathExpression> _PathExpressionCache;
    private readonly List<ResourceIssue> _TrackedIssues;

    // Objects kept for caching and disposal
    private Runspace _Runspace;

    // Track whether Dispose has been called.
    private bool _Disposed;

    internal PSRuleOption Option;

    internal readonly Dictionary<string, Hashtable> LocalizedDataCache;
    internal readonly Dictionary<string, object> ExpressionCache;
    internal readonly Dictionary<string, PSObject[]> ContentCache;
    internal readonly Dictionary<string, SelectorVisitor> Selector;
    internal readonly List<SuppressionGroupVisitor> SuppressionGroup;
    internal readonly IHostContext HostContext;
    internal readonly PipelineInputStream Reader;
    internal readonly string RunId;

    internal readonly Stopwatch RunTime;

    private OptionContext _DefaultOptionContext;

    public System.Security.Cryptography.HashAlgorithm ObjectHashAlgorithm { get; }

    private PipelineContext(PSRuleOption option, IHostContext hostContext, PipelineInputStream reader, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, OptionContextBuilder optionBuilder, IList<ResourceRef> unresolved)
    {
        _OptionBuilder = optionBuilder;
        Option = option;
        HostContext = hostContext;
        Reader = reader;
        _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode.Value;
        _PathExpressionCache = new Dictionary<string, PathExpression>();
        LocalizedDataCache = new Dictionary<string, Hashtable>();
        ExpressionCache = new Dictionary<string, object>();
        ContentCache = new Dictionary<string, PSObject[]>();
        Selector = new Dictionary<string, SelectorVisitor>();
        SuppressionGroup = new List<SuppressionGroupVisitor>();
        _Unresolved = unresolved ?? new List<ResourceRef>();
        _TrackedIssues = new List<ResourceIssue>();

        ObjectHashAlgorithm = option.Execution.HashAlgorithm.GetValueOrDefault(ExecutionOption.Default.HashAlgorithm.Value).GetHashAlgorithm();
        RunId = Environment.GetRunId() ?? ObjectHashAlgorithm.GetDigest(Guid.NewGuid().ToByteArray());
        RunTime = Stopwatch.StartNew();
        _DefaultOptionContext = _OptionBuilder?.Build(null);
    }

    public static PipelineContext New(PSRuleOption option, IHostContext hostContext, PipelineInputStream reader, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, OptionContextBuilder optionBuilder, IList<ResourceRef> unresolved)
    {
        var context = new PipelineContext(option, hostContext, reader, bindTargetName, bindTargetType, bindField, optionBuilder, unresolved);
        CurrentThread = context;
        return context;
    }

    internal sealed class SourceScope
    {
        public readonly ISourceFile File;

        public SourceScope(ISourceFile source)
        {
            File = source;
        }

        public string[] SourceContentCache
        {
            get
            {
                return System.IO.File.ReadAllLines(File.Path, Encoding.UTF8);
            }
        }
    }

    internal Runspace GetRunspace()
    {
        if (_Runspace == null)
        {
            var initialSessionState = Option.Execution.InitialSessionState.GetValueOrDefault(ExecutionOption.Default.InitialSessionState.Value);
            var state = HostState.CreateSessionState(initialSessionState);
            state.LanguageMode = _LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

            _Runspace = RunspaceFactory.CreateRunspace(state);
            Runspace.DefaultRunspace ??= _Runspace;

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
            _Runspace.SessionStateProxy.Path.SetLocation(Environment.GetWorkingPath());
        }
        return _Runspace;
    }

    internal void Import(RunspaceContext context, IResource resource)
    {
        TrackIssue(resource);
        if (TryBaseline(resource, out var baseline) && TryBaselineRef(resource.Id, out var baselineRef))
        {
            RemoveBaselineRef(resource.Id);
            _OptionBuilder.Baseline(baselineRef.Type, baseline.BaselineId, resource.Source.Module, baseline.Spec, baseline.Obsolete);
        }
        else if (resource.Kind == ResourceKind.Selector && resource is SelectorV1 selector)
            Selector[selector.Id.Value] = new SelectorVisitor(context, selector.Id, selector.Source, selector.Spec.If);
        else if (TryModuleConfig(resource, out var moduleConfig))
        {
            if (!string.IsNullOrEmpty(moduleConfig?.Spec?.Rule?.Baseline))
            {
                var baselineId = ResourceHelper.GetIdString(moduleConfig.Source.Module, moduleConfig.Spec.Rule.Baseline);
                if (!_OptionBuilder.ContainsBaseline(baselineId))
                    _Unresolved.Add(new BaselineRef(baselineId, ScopeType.Baseline));
            }
            _OptionBuilder.ModuleConfig(resource.Source.Module, moduleConfig?.Spec);
        }
        else if (resource.Kind == ResourceKind.SuppressionGroup && resource is SuppressionGroupV1 suppressionGroup)
        {
            if (!suppressionGroup.Spec.ExpiresOn.HasValue || suppressionGroup.Spec.ExpiresOn.Value > DateTime.UtcNow)
            {
                SuppressionGroup.Add(new SuppressionGroupVisitor(
                    context: context,
                    id: suppressionGroup.Id,
                    source: suppressionGroup.Source,
                    spec: suppressionGroup.Spec,
                    info: suppressionGroup.Info
                ));
            }
            else
            {
                context.SuppressionGroupExpired(suppressionGroup.Id);
            }
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
        if (r is not BaselineRef br)
            return false;

        baselineRef = br;
        return true;
    }

    private void RemoveBaselineRef(ResourceId resourceId)
    {
        foreach (var r in _Unresolved.ToArray())
        {
            if (ResourceIdEqualityComparer.IdEquals(r.Id, resourceId.Value))
                _Unresolved.Remove(r);
        }
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
        ReportUnresolved(runspaceContext);
        ReportIssue(runspaceContext);
        _DefaultOptionContext = _OptionBuilder.Build(null);
        _OptionBuilder.CheckObsolete(runspaceContext);
    }

    internal void UpdateLanguageScope(ILanguageScope languageScope)
    {
        var context = _OptionBuilder.Build(languageScope.Name);
        languageScope.Configure(context);
    }

    internal int GetConventionOrder(IConvention x)
    {
        return _DefaultOptionContext.GetConventionOrder(x);
    }

    private void ReportUnresolved(RunspaceContext runspaceContext)
    {
        foreach (var unresolved in _Unresolved)
            runspaceContext.ErrorResourceUnresolved(unresolved.Kind, unresolved.Id);

        if (_Unresolved.Count > 0)
            throw new PipelineBuilderException(PSRuleResources.ErrorPipelineException);
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
                ObjectHashAlgorithm?.Dispose();
                _Runspace?.Dispose();
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
