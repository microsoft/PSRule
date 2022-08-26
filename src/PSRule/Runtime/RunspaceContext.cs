// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using static PSRule.Pipeline.PipelineContext;

namespace PSRule.Runtime
{
    [Flags]
    internal enum RunspaceScope
    {
        None = 0,

        Source = 1,

        /// <summary>
        /// Executing a rule.
        /// </summary>
        Rule = 2,

        /// <summary>
        /// Executing a rule precondition.
        /// </summary>
        Precondition = 4,

        /// <summary>
        /// Execution is currently parsing YAML objects.
        /// </summary>
        Resource = 8,

        ConventionBegin = 16,
        ConventionProcess = 32,
        ConventionEnd = 64,
        ConventionInitialize = 128,

        Convention = ConventionInitialize | ConventionBegin | ConventionProcess | ConventionEnd,
        Target = Rule | Precondition | ConventionBegin | ConventionProcess,
        Runtime = Rule | Precondition | Convention,
        All = Source | Rule | Precondition | Resource | Convention,
    }

    /// <summary>
    /// A context for a PSRule runspace.
    /// </summary>
    internal sealed class RunspaceContext : IDisposable
    {
        private const string SOURCE_OUTCOME_FAIL = "Rule.Outcome.Fail";
        private const string SOURCE_OUTCOME_PASS = "Rule.Outcome.Pass";
        private const string ERRORID_INVALIDRULERESULT = "PSRule.Runtime.InvalidRuleResult";
        private const string WARN_KEY_SEPARATOR = "_";

        [ThreadStatic]
        internal static RunspaceContext CurrentThread;

        internal readonly PipelineContext Pipeline;
        internal readonly IPipelineWriter Writer;

        // Fields exposed to engine
        internal RuleRecord RuleRecord;
        internal RuleBlock RuleBlock;
        internal ITargetBindingResult Binding;

        private readonly bool _InconclusiveWarning;
        private readonly bool _NotProcessedWarning;
        private readonly bool _SuppressedRuleWarning;
        private readonly bool _InvariantCultureWarning;
        private readonly OutcomeLogStream _FailStream;
        private readonly OutcomeLogStream _PassStream;
        private readonly Stack<RunspaceScope> _Scope;

        /// <summary>
        /// Track common warnings, to only raise once.
        /// </summary>
        private readonly HashSet<string> _WarnOnce;

        private bool _RaisedUsingInvariantCulture;

        // Pipeline logging
        private string _LogPrefix;
        private int _ObjectNumber;
        private int _RuleErrors;

        private readonly Stopwatch _RuleTimer;
        private readonly List<ResultReason> _Reason;
        private readonly List<IConvention> _Conventions;
        private readonly LanguageScopeSet _LanguageScopes;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal RunspaceContext(PipelineContext pipeline, IPipelineWriter writer)
        {
            Writer = writer;
            CurrentThread = this;
            Pipeline = pipeline;

            _InconclusiveWarning = Pipeline.Option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = Pipeline.Option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;
            _SuppressedRuleWarning = Pipeline.Option.Execution.SuppressedRuleWarning ?? ExecutionOption.Default.SuppressedRuleWarning.Value;
            _InvariantCultureWarning = Pipeline.Option.Execution.InvariantCultureWarning ?? ExecutionOption.Default.InvariantCultureWarning.Value;
            _FailStream = Pipeline.Option.Logging.RuleFail ?? LoggingOption.Default.RuleFail.Value;
            _PassStream = Pipeline.Option.Logging.RulePass ?? LoggingOption.Default.RulePass.Value;
            _WarnOnce = new HashSet<string>();

            _ObjectNumber = -1;
            _RuleTimer = new Stopwatch();
            _Reason = new List<ResultReason>();
            _Conventions = new List<IConvention>();
            _LanguageScopes = new LanguageScopeSet(this);
            _Scope = new Stack<RunspaceScope>();
        }

        internal bool HadErrors => _RuleErrors > 0;

        internal IEnumerable<InvokeResult> Output { get; private set; }

        internal TargetObject TargetObject { get; private set; }

        internal ITargetBinder TargetBinder { get; private set; }

        internal SourceScope Source { get; private set; }

        internal ILanguageScope LanguageScope
        {
            get
            {
                return _LanguageScopes.Current;
            }
        }

        internal bool IsScope(RunspaceScope scope)
        {
            if (scope == RunspaceScope.None && (_Scope == null || _Scope.Count == 0))
                return true;

            if (_Scope == null || _Scope.Count == 0)
                return false;

            var current = _Scope.Peek();
            return scope.HasFlag(current);
        }

        internal void PushScope(RunspaceScope scope)
        {
            _Scope.Push(scope);
        }

        internal void PopScope(RunspaceScope scope)
        {
            var current = _Scope.Peek();
            if (current != scope)
                throw new RuntimeScopeException();

            _Scope.Pop();
        }

        public void Pass()
        {
            if (Writer == null || _PassStream == OutcomeLogStream.None)
                return;

            if (_PassStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Binding.TargetName);

            if (_PassStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(
                    new RuleException(string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.OutcomeRulePass,
                        RuleRecord.RuleName,
                        Binding.TargetName)),
                    SOURCE_OUTCOME_PASS,
                    ErrorCategory.InvalidData,
                    null));

            if (_PassStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(
                    messageData: string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.OutcomeRulePass,
                        RuleRecord.RuleName,
                        Binding.TargetName),
                    source: SOURCE_OUTCOME_PASS));
        }

        public void Fail()
        {
            if (Writer == null || _FailStream == OutcomeLogStream.None)
                return;

            if (_FailStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Binding.TargetName);

            if (_FailStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(
                    new RuleException(string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.OutcomeRuleFail,
                        RuleRecord.RuleName,
                        Binding.TargetName)),
                    SOURCE_OUTCOME_FAIL,
                    ErrorCategory.InvalidData,
                    null));

            if (_FailStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(
                    messageData: string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.OutcomeRuleFail,
                        RuleRecord.RuleName,
                        Binding.TargetName),
                    source: SOURCE_OUTCOME_FAIL));
        }

        public void WarnRuleInconclusive(string ruleId)
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_InconclusiveWarning)
                return;

            Writer.WriteWarning(PSRuleResources.RuleInconclusive, ruleId, Binding.TargetName);
        }

        public void WarnObjectNotProcessed()
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_NotProcessedWarning)
                return;

            Writer.WriteWarning(PSRuleResources.ObjectNotProcessed, Binding.TargetName);
        }

        public void WarnRuleSuppressed(string ruleId)
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_SuppressedRuleWarning)
            {
                return;
            }

            Writer.WriteWarning(PSRuleResources.RuleSuppressed, ruleId, Binding.TargetName);
        }

        public void WarnRuleCountSuppressed(int ruleCount)
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_SuppressedRuleWarning)
                return;

            Writer.WriteWarning(PSRuleResources.RuleCountSuppressed, ruleCount, Binding.TargetName);
        }

        public void WarnRuleSuppressionGroup(string ruleId, ISuppressionInfo suppression)
        {
            if (Writer == null || suppression == null || !Writer.ShouldWriteWarning() || !_SuppressedRuleWarning)
                return;

            if (suppression.Synopsis != null && suppression.Synopsis.HasValue)
                Writer.WriteWarning(PSRuleResources.RuleSuppressionGroupExtended, ruleId, suppression.Id, Binding.TargetName, suppression.Synopsis.Text);
            else
                Writer.WriteWarning(PSRuleResources.RuleSuppressionGroup, ruleId, suppression.Id, Binding.TargetName);
        }

        public void WarnRuleSuppressionGroupCount(ISuppressionInfo suppression, int count)
        {
            if (Writer == null || suppression == null || !Writer.ShouldWriteWarning() || !_SuppressedRuleWarning)
                return;

            if (suppression.Synopsis != null && suppression.Synopsis.HasValue)
                Writer.WriteWarning(PSRuleResources.RuleSuppressionGroupExtendedCount, count, suppression.Id, Binding.TargetName, suppression.Synopsis.Text);
            else
                Writer.WriteWarning(PSRuleResources.RuleSuppressionGroupCount, count, suppression.Id, Binding.TargetName);
        }

        public void ErrorInvaildRuleResult()
        {
            if (Writer == null || !Writer.ShouldWriteError())
                return;

            Writer.WriteError(new ErrorRecord(
                exception: new RuleException(message: string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    PSRuleResources.InvalidRuleResult,
                    RuleBlock.Id
                )),
                errorId: ERRORID_INVALIDRULERESULT,
                errorCategory: ErrorCategory.InvalidResult,
                targetObject: null
            ));
        }

        public void VerboseRuleDiscovery(string path)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose() || string.IsNullOrEmpty(path))
                return;

            Writer.WriteVerbose($"[PSRule][D] -- Discovering rules in: {path}");
        }

        public void VerboseFoundResource(string name, string moduleName, string scriptName)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            var m = string.IsNullOrEmpty(moduleName) ? "." : moduleName;
            Writer.WriteVerbose($"[PSRule][D] -- Found {m}\\{name} in {scriptName}");
        }

        public void VerboseObjectStart()
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), " :: ", Binding.TargetName));
        }

        public void VerboseConditionMessage(string condition, string message, params object[] args)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(
                GetLogPrefix(),
                "[",
                condition,
                "] -- ",
                string.Format(Thread.CurrentThread.CurrentCulture, message, args)));
        }

        public void VerboseConditionResult(string condition, int pass, int count, bool outcome)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- [", pass, "/", count, "] [", outcome, "]"));
        }

        public void VerboseConditionResult(string condition, bool outcome)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- [", outcome, "]"));
        }

        public void VerboseConditionResult(int pass, int count, RuleOutcome outcome)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), " -- [", pass, "/", count, "] [", outcome, "]"));
        }

        public void WriteError(ErrorRecord record)
        {
            if (Writer == null || !Writer.ShouldWriteError())
                return;

            Writer.WriteError(errorRecord: record);
        }

        public void WriteError(ParseError error)
        {
            if (Writer == null || !Writer.ShouldWriteError())
                return;

            var record = new ErrorRecord
            (
                exception: new Pipeline.ParseException(message: error.Message, errorId: error.ErrorId),
                errorId: error.ErrorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            );
            Writer.WriteError(errorRecord: record);
        }

        internal PowerShell GetPowerShell()
        {
            var result = PowerShell.Create();
            result.Runspace = Pipeline.GetRunspace();
            EnableLogging(result);
            return result;
        }

        private static void EnableLogging(PowerShell ps)
        {
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Warning_DataAdded;
            ps.Streams.Verbose.DataAdded += Verbose_DataAdded;
            ps.Streams.Information.DataAdded += Information_DataAdded;
            ps.Streams.Debug.DataAdded += Debug_DataAdded;
        }

        private static void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Writer == null)
            {
                return;
            }
            var collection = sender as PSDataCollection<DebugRecord>;
            var record = collection[e.Index];
            CurrentThread.Writer.WriteDebug(debugRecord: record);
        }

        private static void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Writer == null)
                return;

            var collection = sender as PSDataCollection<InformationRecord>;
            var record = collection[e.Index];
            CurrentThread.Writer.WriteInformation(informationRecord: record);
        }

        private static void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Writer == null)
                return;

            var collection = sender as PSDataCollection<VerboseRecord>;
            var record = collection[e.Index];
            CurrentThread.Writer.WriteVerbose(record.Message);
        }

        private static void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Writer == null)
                return;

            var collection = sender as PSDataCollection<WarningRecord>;
            var record = collection[e.Index];
            CurrentThread.Writer.WriteWarning(message: record.Message);
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            CurrentThread._RuleErrors++;
            if (CurrentThread.Writer == null)
                return;

            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];
            CurrentThread.Error(record);
        }

        public void Error(ActionPreferenceStopException ex)
        {
            if (ex == null)
                return;

            Error(ex.ErrorRecord);
        }

        public void Error(Exception ex)
        {
            if (ex == null)
                return;

            var errorRecord = ex is IContainsErrorRecord error ? error.ErrorRecord : null;
            var scriptStackTrace = errorRecord != null ? GetStackTrace(errorRecord) : null;
            var category = errorRecord != null ? errorRecord.CategoryInfo.Category : ErrorCategory.NotSpecified;
            var errorId = errorRecord != null ? GetErrorId(errorRecord) : null;
            if (RuleRecord == null)
            {
                Writer.WriteError(errorRecord);
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
            if (RuleRecord == null)
            {
                Writer.WriteError(error);
                return;
            }
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
                    Environment.NewLine,
                    string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.RuleStackTrace,
                        RuleBlock.Name,
                        RuleBlock.Extent.File,
                        RuleBlock.Extent.Line)
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

        private static string GetPositionMessage(ErrorRecord errorRecord)
        {
            return errorRecord?.InvocationInfo?.PositionMessage;
        }

        private static IScriptExtent GetErrorScriptExtent(ErrorRecord errorRecord)
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

            var lines = positionMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length != 3 ? 0 : lines[2].LastIndexOf('~') - 1;
        }

        private string GetLogPrefix()
        {
            if (_LogPrefix == null)
                _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";

            return _LogPrefix ?? string.Empty;
        }

        internal SourceScope EnterSourceScope(SourceFile source)
        {
            // TODO: Look at scope caching, and a scope stack.

            if (!source.Exists())
                throw new FileNotFoundException(PSRuleResources.ScriptNotFound, source.Path);

            if (Source != null && Source.File == source)
                return Source;

            _LanguageScopes.UseScope(source.Module);

            // Change scope
            Pipeline.Baseline.UseScope(moduleName: source.Module);
            Source = new SourceScope(source, File.ReadAllLines(source.Path, Encoding.UTF8));
            return Source;
        }

        internal void ExitSourceScope()
        {
            // Look at scope poping and validation.

            Source = null;
        }

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        internal void EnterTargetObject(TargetObject targetObject)
        {
            _ObjectNumber++;
            TargetObject = targetObject;
            TargetBinder.Bind(TargetObject);
            if (Pipeline.ContentCache.Count > 0)
                Pipeline.ContentCache.Clear();

            // Run conventions
            RunConventionBegin();
        }

        public void ExitTargetObject()
        {
            RunConventionProcess();
            TargetObject = null;
        }

        public bool TrySelector(string name)
        {
            return TrySelector(ResourceHelper.GetRuleId(Source.File.Module, name, ResourceIdKind.Unknown));
        }

        public bool TrySelector(ResourceId id)
        {
            if (TargetObject == null || Pipeline == null || !Pipeline.Selector.TryGetValue(id.Value, out var selector))
                return false;

            var annotation = TargetObject.GetAnnotation<SelectorTargetAnnotation>();
            if (annotation.TryGetSelectorResult(selector, out var result))
                return result;

            result = selector.Match(TargetObject.Value);
            annotation.SetSelectorResult(selector, result);
            return result;
        }

        /// <summary>
        /// Enter the rule block scope.
        /// </summary>
        public RuleRecord EnterRuleBlock(RuleBlock ruleBlock)
        {
            Binding = TargetBinder.Result(ruleBlock.Info.ModuleName);

            _RuleErrors = 0;
            RuleBlock = ruleBlock;
            RuleRecord = new RuleRecord(
                runId: Pipeline.RunId,
                ruleId: ruleBlock.Id,
                @ref: ruleBlock.Ref.GetValueOrDefault().Name,
                targetObject: TargetObject,
                targetName: Binding.TargetName,
                targetType: Binding.TargetType,
                tag: ruleBlock.Tag,
                info: ruleBlock.Info,
                field: Binding.Field,
                level: ruleBlock.Level,
                extent: ruleBlock.Extent
            );

            if (Writer != null)
                Writer.EnterScope(ruleBlock.Name);

            // Starts rule execution timer
            _RuleTimer.Restart();
            return RuleRecord;
        }

        /// <summary>
        /// Exit the rule block scope.
        /// </summary>
        public void ExitRuleBlock()
        {
            // Stop rule execution time
            _RuleTimer.Stop();
            RuleRecord.Time = _RuleTimer.ElapsedMilliseconds;

            if (!RuleRecord.IsSuccess())
                for (var i = 0; i < _Reason.Count; i++)
                    RuleRecord._Detail.Add(_Reason[i]);

            if (Writer != null)
                Writer.ExitScope();

            _LogPrefix = null;
            RuleRecord = null;
            RuleBlock = null;
            _RuleErrors = 0;
            _Reason.Clear();
        }

        internal void Import(IConvention resource)
        {
            _Conventions.Add(resource);
        }

        internal void AddService(string id, object service)
        {
            ResourceHelper.ParseIdString(_LanguageScopes.Current.Name, id, out var scopeName, out var name);
            if (!StringComparer.OrdinalIgnoreCase.Equals(_LanguageScopes.Current.Name, scopeName))
                return;

            _LanguageScopes.Current.AddService(name, service);
        }

        internal object GetService(string id)
        {
            ResourceHelper.ParseIdString(_LanguageScopes.Current.Name, id, out var scopeName, out var name);
            return !_LanguageScopes.TryScope(scopeName, out var scope) ? null : scope.GetService(name);
        }

        private void RunConventionInitialize()
        {
            if (IsEmptyConventions())
                return;

            for (var i = 0; i < _Conventions.Count; i++)
                _Conventions[i].Initialize(this, null);
        }

        private void RunConventionBegin()
        {
            if (IsEmptyConventions())
                return;

            for (var i = 0; i < _Conventions.Count; i++)
                _Conventions[i].Begin(this, null);
        }

        private void RunConventionProcess()
        {
            if (IsEmptyConventions())
                return;

            for (var i = 0; i < _Conventions.Count; i++)
                _Conventions[i].Process(this, null);
        }

        private void RunConventionEnd()
        {
            if (IsEmptyConventions())
                return;

            for (var i = 0; i < _Conventions.Count; i++)
                _Conventions[i].End(this, null);
        }

        private bool IsEmptyConventions()
        {
            return _Conventions == null || _Conventions.Count == 0;
        }

        internal void WriteReason(ResultReason[] reason)
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

        public void Init(Source[] source)
        {
            InitLanguageScopes(source);
            Host.HostHelper.ImportResource(source, this);
            foreach (var languageScope in _LanguageScopes.Get())
                Pipeline.Baseline.BuildScope(languageScope);
        }

        private void InitLanguageScopes(Source[] source)
        {
            for (var i = 0; source != null && i < source.Length; i++)
                _LanguageScopes.Import(source[i].Scope, out _);
        }

        public void Begin()
        {
            Pipeline.Begin(this);

            var builder = new TargetBinderBuilder(
                Pipeline.BindTargetName,
                Pipeline.BindTargetType,
                Pipeline.BindField,
                Pipeline.Option.Input.TargetType);

            HashSet<string> _TypeFilter = null;
            if (Pipeline.Option.Input.TargetType != null && Pipeline.Option.Input.TargetType.Length > 0)
                _TypeFilter = new HashSet<string>(Pipeline.Option.Input.TargetType, StringComparer.OrdinalIgnoreCase);

            foreach (var languageScope in _LanguageScopes.Get())
            {
                var targetBinding = Pipeline.Baseline.GetTargetBinding();
                builder.With(new TargetBinder.TargetBindingContext(languageScope.Name, targetBinding, Pipeline.BindTargetName, Pipeline.BindTargetType, Pipeline.BindField, _TypeFilter));
            }
            TargetBinder = builder.Build();
            RunConventionInitialize();
        }

        public void End(IEnumerable<InvokeResult> output)
        {
            Output = output;
            RunConventionEnd();
        }

        public string GetLocalizedPath(string file)
        {
            if (string.IsNullOrEmpty(Source.File.HelpPath))
                return null;

            var culture = Pipeline.Baseline.GetCulture();
            if (!_RaisedUsingInvariantCulture &&
                (culture == null || culture.Length == 0) &&
                _InvariantCultureWarning)
            {
                Writer.WarnUsingInvariantCulture();
                _RaisedUsingInvariantCulture = true;
                return null;
            }

            for (var i = 0; culture != null && i < culture.Length; i++)
            {
                var path = Path.Combine(Source.File.HelpPath, culture[i], file);
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        internal bool ShouldWarnOnce(params string[] key)
        {
            var combinedKey = string.Join(WARN_KEY_SEPARATOR, key);
            if (_WarnOnce.Contains(combinedKey))
                return false;

            _WarnOnce.Add(combinedKey);
            return true;
        }

        #region Configuration

        internal bool TryGetConfigurationValue(string name, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(name))
                return false;

            // Get from baseline configuration
            if (LanguageScope.TryConfigurationValue(name, out var result))
            {
                value = result;
                return true;
            }

            // Check if value exists in Rule definition defaults
            if (RuleBlock == null || RuleBlock.Configuration == null || !RuleBlock.Configuration.ContainsKey(name))
                return false;

            // Get from rule default
            value = RuleBlock.Configuration[name];
            return true;
        }

        #endregion Configuration

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
                    for (var i = 0; _Conventions != null && i < _Conventions.Count; i++)
                    {
                        if (_Conventions[i] is IDisposable d)
                            d.Dispose();
                    }
                    _LanguageScopes.Dispose();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
