// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading;
using static PSRule.Pipeline.PipelineContext;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A context for a PSRule runspace.
    /// </summary>
    internal sealed class RunspaceContext : IDisposable
    {
        private const string SOURCE_OUTCOME_FAIL = "Rule.Outcome.Fail";
        private const string SOURCE_OUTCOME_PASS = "Rule.Outcome.Pass";
        private const string ERRORID_INVALIDRULERESULT = "PSRule.Runtime.InvalidRuleResult";
        private const string WARN_KEY_PROPERTY = "Property";
        private const string WARN_KEY_SEPARATOR = "_";

        [ThreadStatic]
        internal static RunspaceContext CurrentThread;

        internal readonly PipelineContext Pipeline;
        internal readonly PipelineWriter Writer;

        // Fields exposed to engine
        internal RuleRecord RuleRecord;
        internal PSObject TargetObject;
        internal RuleBlock RuleBlock;
        
        internal SourceScope Source;

        private readonly bool _InconclusiveWarning;
        private readonly bool _NotProcessedWarning;
        private readonly OutcomeLogStream _FailStream;
        private readonly OutcomeLogStream _PassStream;

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
        private readonly List<string> _Reason;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal RunspaceContext(PipelineContext pipeline, PipelineWriter writer)
        {
            Writer = writer;
            CurrentThread = this;
            Pipeline = pipeline;

            _InconclusiveWarning = Pipeline.Option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = Pipeline.Option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;
            _FailStream = Pipeline.Option.Logging.RuleFail ?? LoggingOption.Default.RuleFail.Value;
            _PassStream = Pipeline.Option.Logging.RulePass ?? LoggingOption.Default.RulePass.Value;
            _WarnOnce = new HashSet<string>();

            _ObjectNumber = -1;
            _RuleTimer = new Stopwatch();
            _Reason = new List<string>();
        }

        public bool HadErrors
        {
            get { return _RuleErrors > 0; }
        }

        public void Pass()
        {
            if (Writer == null || _PassStream == OutcomeLogStream.None)
                return;

            if (_PassStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName);

            if (_PassStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName)), SOURCE_OUTCOME_PASS, ErrorCategory.InvalidData, null));

            if (_PassStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(messageData: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName), source: SOURCE_OUTCOME_PASS));
        }

        public void Fail()
        {
            if (Writer == null || _FailStream == OutcomeLogStream.None)
                return;

            if (_FailStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName);

            if (_FailStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName)), SOURCE_OUTCOME_FAIL, ErrorCategory.InvalidData, null));

            if (_FailStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(messageData: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName), source: SOURCE_OUTCOME_FAIL));
        }

        public void WarnRuleInconclusive(string ruleId)
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_InconclusiveWarning)
                return;

            Writer.WriteWarning(PSRuleResources.RuleInconclusive, ruleId, Pipeline.Binder.TargetName);
        }

        public void WarnObjectNotProcessed()
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_NotProcessedWarning)
                return;

            Writer.WriteWarning(PSRuleResources.ObjectNotProcessed, Pipeline.Binder.TargetName);
        }

        public void WarnRuleNotFound()
        {
            if (Writer == null || !Writer.ShouldWriteWarning())
                return;

            Writer.WriteWarning(PSRuleResources.RuleNotFound);
        }

        public void WarnBaselineObsolete(string baselineId)
        {
            if (Writer == null || !Writer.ShouldWriteWarning())
                return;

            Writer.WriteWarning(PSRuleResources.BaselineObsolete, baselineId);
        }

        public void WarnPropertyObsolete(string variableName, string propertyName)
        {
            DebugPropertyObsolete(variableName, propertyName);
            if (Writer == null || !Writer.ShouldWriteWarning() || !ShouldWarnOnce(WARN_KEY_PROPERTY, variableName, propertyName))
                return;

            Writer.WriteWarning(PSRuleResources.PropertyObsolete, variableName, propertyName);
        }

        private void DebugPropertyObsolete(string variableName, string propertyName)
        {
            if (Writer == null || !Writer.ShouldWriteDebug())
                return;

            Writer.WriteDebug(PSRuleResources.DebugPropertyObsolete, RuleBlock.RuleName, variableName, propertyName);
        }

        public void ErrorInvaildRuleResult()
        {
            if (Writer == null || !Writer.ShouldWriteError())
                return;

            Writer.WriteError(new ErrorRecord(
                exception: new RuleException(message: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidRuleResult, RuleBlock.RuleId)),
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

        public void VerboseFoundRule(string ruleName, string moduleName, string scriptName)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            var m = string.IsNullOrEmpty(moduleName) ? "." : moduleName;
            Writer.WriteVerbose($"[PSRule][D] -- Found {m}\\{ruleName} in {scriptName}");
        }

        public void VerboseObjectStart()
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), " :: ", Pipeline.Binder.TargetName));
        }

        public void VerboseConditionMessage(string condition, string message, params object[] args)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- ", string.Format(Thread.CurrentThread.CurrentCulture, message, args)));
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
                exception: new ParseException(message: error.Message, errorId: error.ErrorId),
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
            if (RuleBlock == null)
                return record.ScriptStackTrace;

            return string.Concat(
                record.ScriptStackTrace,
                Environment.NewLine,
                string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RuleStackTrace, RuleBlock.RuleName, RuleBlock.Extent.File, RuleBlock.Extent.StartLineNumber)
            );
        }

        private string GetErrorId(ErrorRecord record)
        {
            if (RuleBlock == null)
                return record.FullyQualifiedErrorId;

            return string.Concat(
                record.FullyQualifiedErrorId,
                ",",
                RuleBlock.RuleName
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
            if (lines.Length != 3)
                return 0;

            return lines[2].LastIndexOf('~') - 1;
        }

        private string GetLogPrefix()
        {
            if (_LogPrefix == null)
                _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";

            return _LogPrefix ?? string.Empty;
        }

        internal SourceScope EnterSourceScope(SourceFile source)
        {
            if (!source.Exists())
                throw new FileNotFoundException(PSRuleResources.ScriptNotFound, source.Path);

            if (Source != null && Source.File == source)
                return Source;

            // Change scope
            Pipeline.Baseline.UseScope(moduleName: source.ModuleName);
            Source = new SourceScope(source, File.ReadAllLines(source.Path, Encoding.UTF8), Pipeline.Baseline.RuleFilter(), Pipeline.Baseline.GetConfiguration());
            return Source;
        }

        internal void ExitSourceScope()
        {
            Source = null;
        }

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        public void SetTargetObject(PSObject targetObject)
        {
            _ObjectNumber++;
            TargetObject = targetObject;
            Pipeline.Binder.Bind(Pipeline.Baseline, TargetObject);
            if (Pipeline.ContentCache.Count > 0)
                Pipeline.ContentCache.Clear();
        }

        /// <summary>
        /// Enter the rule block scope.
        /// </summary>
        public RuleRecord EnterRuleBlock(RuleBlock ruleBlock)
        {
            _RuleErrors = 0;
            RuleBlock = ruleBlock;
            RuleRecord = new RuleRecord(
                ruleId: ruleBlock.RuleId,
                ruleName: ruleBlock.RuleName,
                targetObject: TargetObject,
                targetName: Pipeline.Binder.TargetName,
                targetType: Pipeline.Binder.TargetType,
                tag: ruleBlock.Tag,
                info: ruleBlock.Info,
                field: Pipeline.Binder.Field
            );

            if (Writer != null)
                Writer.EnterScope(ruleBlock.RuleName);

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
                RuleRecord.Reason = _Reason.ToArray();

            if (Writer != null)
                Writer.ExitScope();

            _LogPrefix = null;
            RuleRecord = null;
            RuleBlock = null;
            _RuleErrors = 0;
            _Reason.Clear();
        }

        public void WriteReason(string text)
        {
            if (string.IsNullOrEmpty(text) || Pipeline.ExecutionScope != ExecutionScope.Condition)
                return;

            _Reason.Add(text);
        }

        public void Begin()
        {
            Pipeline.Baseline.Init(this);
        }

        public string GetLocalizedPath(string file)
        {
            if (string.IsNullOrEmpty(Source.File.HelpPath))
                return null;

            var culture = Pipeline.Baseline.GetCulture();
            if (!_RaisedUsingInvariantCulture && (culture == null || culture.Length == 0))
            {
                Writer.WarnUsingInvariantCulture();
                _RaisedUsingInvariantCulture = true;
                return null;
            }
            
            for (var i = 0; i < culture.Length; i++)
            {
                var path = Path.Combine(Source.File.HelpPath, culture[i], file);
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        private bool ShouldWarnOnce(params string[] key)
        {
            var combinedKey = string.Join(WARN_KEY_SEPARATOR, key);
            if (_WarnOnce.Contains(combinedKey))
                return false;

            _WarnOnce.Add(combinedKey);
            return true;
        }

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
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
