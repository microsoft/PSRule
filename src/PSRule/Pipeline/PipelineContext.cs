// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.Rules;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Host;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Context applicable to the whole pipeline, including during early stage setup.
/// </summary>
internal sealed class PipelineContext : IPipelineContext, IBindingContext
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string DebugPreference = "DebugPreference";

    [ThreadStatic]
    internal static PipelineContext? CurrentThread;

    internal readonly OptionContextBuilder OptionBuilder;

    // Configuration parameters

    private readonly LanguageMode _LanguageMode;
    private readonly Dictionary<string, PathExpression> _PathExpressionCache;

    // Objects kept for caching and disposal
    private Runspace? _Runspace;

    // Track whether Dispose has been called.
    private bool _Disposed;

    public PSRuleOption Option { get; }

    internal readonly Dictionary<string, Hashtable> LocalizedDataCache;
    internal readonly Dictionary<string, object> ExpressionCache;
    internal readonly Dictionary<string, object[]> ContentCache;

    internal IDictionary<string, SelectorVisitor> Selector;

    internal IList<SuppressionGroupVisitor> SuppressionGroup;

    internal readonly IHostContext? HostContext;
    private readonly Func<IPipelineReader> _GetReader;
    internal IPipelineReader? Reader { get; private set; }
    internal readonly string RunInstance;

    internal readonly Stopwatch RunTime;

    public ResourceCache ResourceCache { get; }

    private OptionContext _DefaultOptionContext;

    public System.Security.Cryptography.HashAlgorithm ObjectHashAlgorithm { get; }

    public IPipelineWriter Writer { get; }

    /// <summary>
    /// A set of languages scopes for this pipeline.
    /// </summary>
    public ILanguageScopeSet LanguageScope { get; }

    private PipelineContext(PSRuleOption option, IHostContext? hostContext, Func<IPipelineReader> reader, IPipelineWriter writer, ILanguageScopeSet languageScope, OptionContextBuilder optionBuilder, ResourceCache resourceCache)
    {
        Option = option ?? throw new ArgumentNullException(nameof(option));
        LanguageScope = languageScope ?? throw new ArgumentNullException(nameof(languageScope));

        OptionBuilder = optionBuilder;
        ResourceCache = resourceCache;

        HostContext = hostContext;
        _GetReader = reader;
        Writer = writer;
        _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode!.Value;
        _PathExpressionCache = [];
        LocalizedDataCache = [];
        ExpressionCache = [];
        ContentCache = [];
        Selector = new Dictionary<string, SelectorVisitor>(StringComparer.OrdinalIgnoreCase);
        SuppressionGroup = [];

        ObjectHashAlgorithm = option.Execution.HashAlgorithm.GetValueOrDefault(ExecutionOption.Default.HashAlgorithm!.Value).GetHashAlgorithm();
        RunInstance = Environment.GetRunInstance() ?? ObjectHashAlgorithm.GetDigest(Guid.NewGuid().ToByteArray());
        RunTime = Stopwatch.StartNew();
        _DefaultOptionContext = OptionBuilder.Build(null);
        LanguageScope = languageScope;
    }

    public static PipelineContext New(PSRuleOption option, IHostContext? hostContext, Func<IPipelineReader> reader, IPipelineWriter writer, ILanguageScopeSet languageScope, OptionContextBuilder optionBuilder, ResourceCache resourceCache)
    {
        var context = new PipelineContext(option, hostContext, reader, writer, languageScope, optionBuilder, resourceCache);
        CurrentThread = context;
        return context;
    }

    internal Runspace GetRunspace()
    {
        if (_Runspace == null)
        {
            var initialSessionState = Option.Execution.InitialSessionState.GetValueOrDefault(ExecutionOption.Default.InitialSessionState!.Value);
            var state = HostState.CreateSessionState(initialSessionState, _LanguageMode);

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

    internal void Initialize(LegacyRunspaceContext runspaceContext, Source[] sources)
    {
        // Import PS Language Blocks.
        var blocks = HostHelper.GetPSResources<ILanguageBlock>(sources, runspaceContext);
        var conventions = blocks.ToConventionsV1(runspaceContext);
        var rules = blocks.ToRuleV1(runspaceContext);

        ResourceCache.Import(conventions);
        ResourceCache.Import(rules);

        ReportUnresolved(runspaceContext);
        ReportIssue(runspaceContext);

        // Build selectors
        Selector = ResourceCache.OfType<ISelector>().ToDictionary(key => key.Id.Value, value => value.ToSelectorVisitor());

        // Build suppression groups
        var suppressionGroupFilter = new SuppressionGroupFilter();
        SuppressionGroup = ResourceCache.OfType<ISuppressionGroup>().Where(suppressionGroupFilter.Match).Select(i => i.ToSuppressionGroupVisitor(runspaceContext)).ToList();

        _DefaultOptionContext = OptionBuilder.Build(null);
        OptionBuilder.CheckObsolete(runspaceContext);

        Reader = _GetReader();
    }

    internal void UpdateLanguageScope(ILanguageScope languageScope)
    {
        var context = OptionBuilder.Build(languageScope.Name);
        languageScope.Configure(context);
    }

    internal int GetConventionOrder(IConventionV1 x)
    {
        return _DefaultOptionContext.GetConventionOrder(x);
    }

    private void ReportUnresolved(ILogger logger)
    {
        foreach (var unresolved in ResourceCache.Unresolved)
            logger.ErrorResourceUnresolved(unresolved.Kind, unresolved.Id);

        if (ResourceCache.Unresolved.Any())
            throw new PipelineBuilderException(PSRuleResources.ErrorPipelineException);
    }

    /// <summary>
    /// Report any tracked issues.
    /// </summary>
    private void ReportIssue(LegacyRunspaceContext runspaceContext)
    {
        foreach (var issue in ResourceCache.Issues)
        {
            if (issue.Type == ResourceIssueType.SuppressionGroupExpired)
            {
                runspaceContext.SuppressionGroupExpired(issue.ResourceId);
            }
        }
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
                LanguageScope.Dispose();
                _PathExpressionCache.Clear();
                LanguageScope.Dispose();
                LocalizedDataCache.Clear();
                ExpressionCache.Clear();
                ContentCache.Clear();
                // Reader.Dispose();
                Writer.Dispose();
                RunTime.Stop();
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}

#nullable restore
