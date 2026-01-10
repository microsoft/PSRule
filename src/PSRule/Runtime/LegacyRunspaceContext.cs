// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Language;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.Expressions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Pipeline.Runs;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime.Binding;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Runtime;

/// <summary>
/// A context applicable to rule execution.
/// </summary>
internal sealed class LegacyRunspaceContext : IDisposable, ILogger, IScriptResourceDiscoveryContext, IGetLocalizedPathContext, IExpressionContext, IRunOverrideContext, IRunBuilderContext
{
    private const string ERROR_ID_INVALID_RULE_RESULT = "PSRule.Runtime.InvalidRuleResult";
    private const string WARN_KEY_SEPARATOR = "_";

    private static readonly EventId PSR0022 = new(22, "PSR0022");

    [ThreadStatic]
    internal static LegacyRunspaceContext? CurrentThread;

    internal readonly PipelineContext Pipeline;

    // Fields exposed to engine
    internal RuleRecord? RuleRecord;
    internal IRuleBlock? RuleBlock;
    internal ITargetBindingResult? Binding;

    private readonly ExecutionActionPreference _RuleInconclusive;
    private readonly ExecutionActionPreference _UnprocessedObject;
    private readonly ExecutionActionPreference _RuleSuppressed;
    private readonly ExecutionActionPreference _InvariantCulture;
    private readonly ExecutionActionPreference _DuplicateResourceId;

    ///// <summary>
    ///// Track the current runspace scope.
    ///// </summary>
    //private readonly Stack<RunspaceScope> _Scope;

    /// <summary>
    /// Track common warnings, to only raise once.
    /// </summary>
    private readonly HashSet<string> _WarnOnce;

    private bool _RaisedUsingInvariantCulture;

    // Pipeline logging
    private string? _LogPrefix;
    private int _ObjectNumber;
    private int _RuleErrors;

    private readonly Stopwatch _RuleTimer;
    private readonly List<ResultReason> _Reason;
    private IConventionV1[]? _Conventions;

    // Track whether Dispose has been called.
    private bool _Disposed;

    internal LegacyRunspaceContext(PipelineContext pipeline)
    {
        CurrentThread = this;
        Pipeline = pipeline;

        _RuleInconclusive = Pipeline.Option.Execution.RuleInconclusive.GetValueOrDefault(ExecutionOption.Default.RuleInconclusive!.Value);
        _UnprocessedObject = Pipeline.Option.Execution.UnprocessedObject.GetValueOrDefault(ExecutionOption.Default.UnprocessedObject!.Value);
        _RuleSuppressed = Pipeline.Option.Execution.RuleSuppressed.GetValueOrDefault(ExecutionOption.Default.RuleSuppressed!.Value);
        _InvariantCulture = Pipeline.Option.Execution.InvariantCulture.GetValueOrDefault(ExecutionOption.Default.InvariantCulture!.Value);
        _DuplicateResourceId = Pipeline.Option.Execution?.DuplicateResourceId ?? ExecutionOption.Default.DuplicateResourceId!.Value;

        _WarnOnce = [];

        _ObjectNumber = -1;
        _RuleTimer = new Stopwatch();
        _Reason = [];
        //_Scope = new Stack<RunspaceScope>();
    }

    internal bool HadErrors => _RuleErrors > 0 || Pipeline.RunspaceContext.ErrorCount > 0;

    public IPipelineWriter Writer => Pipeline.Writer;

    public ILogger? Logger => Pipeline.Writer;

    internal IEnumerable<InvokeResult>? Output { get; private set; }

    public IRun? Run { get; private set; }

    internal TargetObject? TargetObject { get; private set; }

    public ISourceFile? Source { get; private set; }

    internal ITargetBinder? TargetBinder { get; private set; }

    public ILanguageScope? Scope { get; private set; }

    string? IResourceContext.Scope => Scope?.Name;

    public ResourceKind Kind => throw new NotImplementedException();

    public string LanguageScope => throw new NotImplementedException();

    public ITargetObject Current => TargetObject;

    public ResourceId? RuleId => ((ILanguageBlock)RuleBlock)?.Id;

    public bool IsScope(RunspaceScope scope) => Pipeline.RunspaceContext?.IsScope(scope) ?? false;

    public void PushScope(RunspaceScope scope) => Pipeline?.RunspaceContext?.PushScope(scope);

    public void PopScope(RunspaceScope scope) => Pipeline?.RunspaceContext.PopScope(scope);

    public void WarnRuleInconclusive(string ruleId)
    {
        this.Throw(_RuleInconclusive, PSRuleResources.RuleInconclusive, ruleId, Binding?.TargetName);
    }

    public void WarnObjectNotProcessed()
    {
        this.Throw(_UnprocessedObject, PSRuleResources.ObjectNotProcessed, Binding?.TargetName);
    }

    public void RuleSuppressed(string ruleId)
    {
        this.Throw(_RuleSuppressed, PSRuleResources.RuleSuppressed, ruleId, Binding?.TargetName);
    }

    public void WarnRuleCountSuppressed(int ruleCount)
    {
        this.Throw(_RuleSuppressed, PSRuleResources.RuleCountSuppressed, ruleCount, Binding?.TargetName);
    }

    public void RuleSuppressionGroup(string ruleId, ISuppressionInfo suppression)
    {
        if (suppression == null)
            return;

        if (suppression.Synopsis != null && suppression.Synopsis.HasValue)
            this.Throw(_RuleSuppressed, PSRuleResources.RuleSuppressionGroupExtended, ruleId, suppression.Id, Binding?.TargetName, suppression.Synopsis.Text);
        else
            this.Throw(_RuleSuppressed, PSRuleResources.RuleSuppressionGroup, ruleId, suppression.Id, Binding?.TargetName);
    }

    public void RuleSuppressionGroupCount(ISuppressionInfo suppression, int count)
    {
        if (suppression == null)
            return;

        if (suppression.Synopsis != null && suppression.Synopsis.HasValue)
            this.Throw(_RuleSuppressed, PSRuleResources.RuleSuppressionGroupExtendedCount, count, suppression.Id, Binding?.TargetName, suppression.Synopsis.Text);
        else
            this.Throw(_RuleSuppressed, PSRuleResources.RuleSuppressionGroupCount, count, suppression.Id, Binding?.TargetName);
    }

    public void ErrorInvalidRuleResult()
    {
        if (Logger == null || !Logger.IsEnabled(LogLevel.Error))
            return;

        Logger.LogError(new ErrorRecord(
            exception: new RuleException(message: string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.InvalidRuleResult,
                ((ILanguageBlock)RuleBlock)?.Id
            )),
            errorId: ERROR_ID_INVALID_RULE_RESULT,
            errorCategory: ErrorCategory.InvalidResult,
            targetObject: null
        ));
    }

    public void VerboseFoundResource(string name, string scope, string scriptName)
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Trace))
            return;

        scope = string.IsNullOrEmpty(scope) ? "." : scope;
        Writer.LogVerbose(EventId.None, "[PSRule][D] -- Found {0}\\{1} in {2}", scope, name, scriptName);
    }

    public void LogObjectStart()
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Debug))
            return;

        Writer.WriteDebug(string.Concat(GetLogPrefix(), " :: ", Binding?.TargetName));
    }

    public void VerboseConditionMessage(string condition, string message, params object[] args)
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Trace))
            return;

        Writer.LogVerbose(EventId.None, string.Concat(
            GetLogPrefix(),
            "[",
            condition,
            "] -- ",
            string.Format(Thread.CurrentThread.CurrentCulture, message, args)));
    }

    public void VerboseConditionResult(string condition, int pass, int count, bool outcome)
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Trace))
            return;

        Writer.LogVerbose(EventId.None, string.Concat(GetLogPrefix(), "[", condition, "] -- [", pass, "/", count, "] [", outcome, "]"));
    }

    public void VerboseConditionResult(string condition, bool outcome)
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Trace))
            return;

        Writer.LogVerbose(EventId.None, string.Concat(GetLogPrefix(), "[", condition, "] -- [", outcome, "]"));
    }

    public void VerboseConditionResult(int pass, int count, RuleOutcome outcome)
    {
        if (Writer == null || !Writer.IsEnabled(LogLevel.Trace))
            return;

        Writer.LogVerbose(EventId.None, string.Concat(GetLogPrefix(), " -- [", pass, "/", count, "] [", outcome, "]"));
    }

    public RestrictScriptSource RestrictScriptSource => Pipeline.RunspaceContext.RestrictScriptSource;

    public PowerShell? GetPowerShell()
    {
        return Pipeline.RunspaceContext.GetPowerShell();
    }

    public void ReportIssue(ResourceIssue issue)
    {
        switch (issue.Type)
        {
            case ResourceIssueType.DuplicateResourceId:
                Logger?.Throw(_DuplicateResourceId, PSRuleResources.DuplicateResourceId, issue.ResourceId, issue.Args![0]);
                break;
            case ResourceIssueType.DuplicateResourceName:
                Logger?.LogWarning(new EventId(0), PSRuleResources.DuplicateRuleName, issue.Args![0]);
                break;
            case ResourceIssueType.RuleExcluded:
                var preference1 = Pipeline.Option.Execution.RuleExcluded ?? ExecutionOption.Default.RuleExcluded!.Value;
                Logger?.Throw(preference1, PSRuleResources.RuleExcluded, issue.ResourceId.Value);
                break;
            case ResourceIssueType.AliasReference:
                var preference2 = Pipeline.Option.Execution.AliasReference ?? ExecutionOption.Default.AliasReference!.Value;
                Logger?.Throw(preference2, PSRuleResources.AliasReference, issue?.Args[1], issue.ResourceId.Value, issue?.Args[0], issue?.Args[2]);
                break;
            default:
                throw new NotImplementedException($"Resource issue '{issue.Type}' is not implemented.");
        }
    }

    public bool Match(IResource resource)
    {
        try
        {
            EnterLanguageScope(resource.Source);
            var filter = Scope!.GetFilter(ResourceKind.Rule);
            return filter == null || filter.Match(resource);
        }
        finally
        {
            ExitLanguageScope(resource.Source);
        }
    }

    public void Error(ActionPreferenceStopException ex)
    {
        if (ex == null) return;

        Error(ex.ErrorRecord);
    }

    public void Error(Exception ex)
    {
        if (ex == null) return;

        var errorRecord = ex is IContainsErrorRecord error ? error.ErrorRecord : null;
        var scriptStackTrace = errorRecord != null ? GetStackTrace(errorRecord) : null;
        var category = errorRecord != null ? errorRecord.CategoryInfo.Category : ErrorCategory.NotSpecified;
        var errorId = errorRecord != null ? GetErrorId(errorRecord) : null;
        if (RuleRecord == null)
        {
            CurrentThread.Logger?.LogError(errorRecord);
            return;
        }
        RuleRecord.Outcome = RuleOutcome.Error;
        RuleRecord.Error = new ErrorInfo(
            message: ex.Message,
            scriptStackTrace: scriptStackTrace,
            errorId: errorId,
            exception: ex,
            category: category,
            positionMessage: GetPositionMessage(errorRecord),
            scriptExtent: GetErrorScriptExtent(errorRecord)
        );
    }

    public void Error(ErrorRecord error)
    {
        Logger?.LogError(error);
        if (RuleRecord == null) return;

        RuleRecord.Outcome = RuleOutcome.Error;
        RuleRecord.Error = new ErrorInfo(
            message: error.Exception?.Message,
            scriptStackTrace: GetStackTrace(error),
            errorId: GetErrorId(error),
            exception: error.Exception,
            category: error.CategoryInfo.Category,
            positionMessage: GetPositionMessage(error),
            scriptExtent: GetErrorScriptExtent(error)
        );
    }

    private string GetStackTrace(ErrorRecord record)
    {
        return RuleBlock == null
            ? record.ScriptStackTrace
            : string.Concat(
                record.ScriptStackTrace,
                System.Environment.NewLine,
                string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    PSRuleResources.RuleStackTrace,
                    RuleBlock.Name,
                    RuleBlock.Extent?.File,
                    RuleBlock.Extent?.Line)
            );
    }

    private string GetErrorId(ErrorRecord record)
    {
        return RuleBlock == null
            ? record.FullyQualifiedErrorId
            : string.Concat(
                record.FullyQualifiedErrorId,
                ",",
                RuleBlock.Name
            );
    }

    private static string? GetPositionMessage(ErrorRecord? errorRecord)
    {
        return errorRecord?.InvocationInfo?.PositionMessage;
    }

    private static ScriptExtent? GetErrorScriptExtent(ErrorRecord? errorRecord)
    {
        if (errorRecord == null)
            return null;

        var startPos = new ScriptPosition(
            errorRecord.InvocationInfo.ScriptName,
            errorRecord.InvocationInfo.ScriptLineNumber,
            errorRecord.InvocationInfo.OffsetInLine,
            errorRecord.InvocationInfo.Line
        );
        var endPos = new ScriptPosition(
            errorRecord.InvocationInfo.ScriptName,
            errorRecord.InvocationInfo.ScriptLineNumber,
            GetPositionMessageOffset(errorRecord.InvocationInfo.PositionMessage),
            errorRecord.InvocationInfo.Line
        );
        return new ScriptExtent(startPos, endPos);
    }

    private static int GetPositionMessageOffset(string positionMessage)
    {
        if (string.IsNullOrEmpty(positionMessage))
            return 0;

        var lines = positionMessage.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Length != 3 ? 0 : lines[2].LastIndexOf('~') - 1;
    }

    private string GetLogPrefix()
    {
        _LogPrefix ??= $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";
        return _LogPrefix ?? string.Empty;
    }

    public void EnterLanguageScope(ISourceFile file)
    {
        // TODO: Look at scope caching, and a scope stack.

        if (!file.Exists())
            throw new FileNotFoundException(PSRuleResources.ScriptNotFound, file.Path);

        if (!Pipeline.LanguageScope.TryScope(file.Module, out var scope))
            throw new RuntimeScopeException(PSR0022, PSRuleResources.PSR0022);

        Scope = scope;

        if (TargetObject != null && Scope != null)
            Binding = Scope.Bind(TargetObject);

        Source = file;
    }

    public void ExitLanguageScope(ISourceFile file)
    {
        // Look at scope popping and validation.
        Scope = null;

        Source = null;
    }

    /// <summary>
    /// Increment the pipeline object number.
    /// </summary>
    internal void EnterTargetObject(IRun run, TargetObject targetObject)
    {
        _ObjectNumber++;
        TargetObject = targetObject;
        if (Pipeline.ContentCache.Count > 0)
            Pipeline.ContentCache.Clear();

        Binding = run.Bind(targetObject);

        // Run conventions
        RunConventionBegin();
    }

    public void ExitTargetObject()
    {
        if (TargetObject == null)
            return;

        RunConventionProcess();
        TargetObject = null;
        Binding = null;
    }

    public bool TrySelector(ResourceId id, ITargetObject targetObject)
    {
        if (targetObject == null || Pipeline == null || !Pipeline.Selector.TryGetValue(id.Value, out var selector))
            return false;

        var annotation = targetObject.GetAnnotation<SelectorTargetAnnotation>() ?? new SelectorTargetAnnotation();
        if (annotation.TryGetSelectorResult(selector, out var result))
            return result;

        result = selector.If(this, targetObject);
        annotation.SetSelectorResult(selector, result);
        targetObject.SetAnnotation(annotation);
        return result;
    }

    public void Reason(IOperand operand, string text, params object[] args)
    {
        WriteReason(new ResultReason(null, operand, text, args));
    }

    public bool GetPathExpression(string path, out PathExpression expression)
    {
        throw new NotImplementedException();
    }

    public void CachePathExpression(string path, PathExpression expression)
    {
        throw new NotImplementedException();
    }

    public void EnterRun(IRun run)
    {
        Run = run;
    }

    /// <summary>
    /// Enter the rule block scope.
    /// </summary>
    public RuleRecord EnterRuleBlock(IRun run, IRuleBlock ruleBlock)
    {
        EnterRun(run);
        EnterLanguageScope(ruleBlock.Source);

        var targetName = TargetObject?.Name ?? Binding?.TargetName ?? "<unknown>";
        var targetType = TargetObject?.Type ?? Binding?.TargetType ?? "<unknown>";

        _RuleErrors = 0;
        RuleBlock = ruleBlock;
        RuleRecord = new RuleRecord(
            run: run,
            ruleId: ((ILanguageBlock)ruleBlock).Id,
            @ref: ((IResource)ruleBlock).Ref.GetValueOrDefault().Name,
            targetObject: TargetObject!,
            targetName: targetName,
            targetType: targetType,
            tag: ruleBlock.Tag,
            info: ruleBlock.Info,
            field: Binding?.Field,
            @default: ruleBlock.Default,
            extent: ruleBlock.Extent,
            @override: ruleBlock.Override
        );

        // Writer?.EnterScope(ruleBlock.Name);

        // Starts rule execution timer
        _RuleTimer.Restart();
        return RuleRecord;
    }

    /// <summary>
    /// Exit the rule block scope.
    /// </summary>
    public void ExitRuleBlock(IRuleBlock ruleBlock)
    {
        // Stop rule execution time
        _RuleTimer.Stop();

        if (RuleRecord != null)
        {
            RuleRecord.Time = _RuleTimer.ElapsedMilliseconds;
            if (!RuleRecord.IsSuccess())
            {
                for (var i = 0; i < _Reason.Count; i++)
                    RuleRecord._Detail.Add(_Reason[i]);
            }
        }

        // Writer?.ExitScope();

        _LogPrefix = null;
        RuleRecord = null;
        RuleBlock = null;
        _RuleErrors = 0;
        _Reason.Clear();

        ExitLanguageScope(ruleBlock.Source);
    }

    internal void AddService(string id, object service)
    {
        if (Scope == null) throw new InvalidOperationException("Can not call out of scope.");

        ResourceHelper.ParseIdString(Scope.Name, id, out var scopeName, out var name);
        if (!StringComparer.OrdinalIgnoreCase.Equals(Scope.Name, scopeName) || string.IsNullOrEmpty(name))
            return;

        Scope.AddService(name!, service);
    }

    internal object? GetService(string id)
    {
        if (Scope == null) throw new InvalidOperationException("Can not call out of scope.");

        ResourceHelper.ParseIdString(Scope.Name, id, out var scopeName, out var name);
        return scopeName == null || !Pipeline.LanguageScope.TryScope(scopeName, out var scope) || scope == null || name == null || string.IsNullOrEmpty(name) ? null : scope.GetService(name);
    }

    private void RunConventionInitialize()
    {
        for (var i = 0; _Conventions != null && i < _Conventions.Length; i++)
            _Conventions[i].Initialize(this, null);
    }

    private void RunConventionBegin()
    {
        for (var i = 0; _Conventions != null && i < _Conventions.Length; i++)
            _Conventions[i].Begin(this, null);
    }

    private void RunConventionProcess()
    {
        for (var i = 0; _Conventions != null && i < _Conventions.Length; i++)
            _Conventions[i].Process(this, null);
    }

    private void RunConventionEnd()
    {
        for (var i = 0; _Conventions != null && i < _Conventions.Length; i++)
            _Conventions[i].End(this, null);
    }

    public void WriteReason(ResultReason[] reason)
    {
        for (var i = 0; reason != null && i < reason.Length; i++)
            WriteReason(reason[i]);
    }

    internal void WriteReason(ResultReason reason)
    {
        if (reason == null || string.IsNullOrEmpty(reason.Text) || !IsScope(RunspaceScope.Rule))
            return;

        _Reason.Add(reason);
    }

    public void Initialize(Source[] source)
    {
        Pipeline.RunspaceContext.EnterResourceContext(this);

        foreach (var languageScope in Pipeline.LanguageScope.Get())
            Pipeline.UpdateLanguageScope(languageScope);

        foreach (var resource in Pipeline.ResourceCache)
        {
            if (resource == null)
                continue;

            EnterLanguageScope(resource.Source);
            try
            {
                Host.HostHelper.UpdateHelpInfo(this, resource);
            }
            finally
            {
                ExitLanguageScope(resource.Source);
            }
        }

        foreach (var languageScope in Pipeline.LanguageScope.Get())
            Pipeline.UpdateLanguageScope(languageScope);

        Pipeline.Initialize(this, source);

        var filter = Pipeline.GetConventionFilter();

        _Conventions = [.. Pipeline.ResourceCache.OfType<IConventionV1>().Where(c => filter == null || filter.Match(c))];
        Array.Sort(_Conventions, new ConventionComparer(Pipeline.GetConventionOrder));

        RunConventionInitialize();
    }

    public void Begin()
    {
        // Do nothing.
    }

    public void End(IEnumerable<InvokeResult> output)
    {
        Output = output;
        RunConventionEnd();

        Pipeline.RunspaceContext.ExitResourceContext(this);
    }

    /// <summary>
    /// Try to bind the scope of the object.
    /// </summary>
    public bool TryGetScope(object o, out string[]? scope)
    {
        if (TargetObject != null && TargetObject.Value == o)
        {
            scope = TargetObject.Scope;
            return true;
        }
        scope = null;
        return false;
    }

    public string? GetLocalizedPath(string file, out string? culture)
    {
        culture = null;
        if (string.IsNullOrEmpty(Source?.HelpPath))
            return null;

        var cultures = Scope?.Culture;
        if (!_RaisedUsingInvariantCulture && (cultures == null || cultures.Length == 0))
        {
            this.Throw(_InvariantCulture, PSRuleResources.UsingInvariantCulture);
            _RaisedUsingInvariantCulture = true;
            return null;
        }

        if (cultures == null || cultures.Length == 0)
            return null;

        return new LocalizedFileSearch(cultures).GetLocalizedPath(Source!.HelpPath, file, out culture);
    }

    internal bool ShouldWarnOnce(params string[] key)
    {
        var combinedKey = string.Join(WARN_KEY_SEPARATOR, key);
        if (_WarnOnce.Contains(combinedKey))
            return false;

        _WarnOnce.Add(combinedKey);
        return true;
    }

    public bool TryGetOverride(ResourceId id, out RuleOverride? propertyOverride)
    {
        propertyOverride = null;
        return Scope?.TryGetOverride(id, out propertyOverride) ?? false;
    }

    #region Configuration

    public bool TryGetConfigurationValue(string name, out object? value)
    {
        value = null;
        if (string.IsNullOrEmpty(name))
            return false;

        // Get from run.
        if (Run != null && Run.TryConfigurationValue(name, out var result))
        {
            value = result;
            return true;
        }

        // Check if value exists in Rule definition defaults.
        if (RuleBlock == null || RuleBlock.Configuration == null || !RuleBlock.Configuration.ContainsKey(name))
            return false;

        // Get from rule default.
        value = RuleBlock.Configuration[name];
        return true;
    }

    #endregion Configuration

    #region ILogger

    /// <inheritdoc/>
    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return Logger?.IsEnabled(logLevel) ?? false;
    }

    /// <inheritdoc/>
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logger?.Log(logLevel, eventId, state, exception, formatter);
    }

    #endregion ILogger

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
                _RuleTimer.Stop();
                _Reason.Clear();
                for (var i = 0; _Conventions != null && i < _Conventions.Length; i++)
                {
                    if (_Conventions[i] is IDisposable d)
                        d.Dispose();
                }
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}
