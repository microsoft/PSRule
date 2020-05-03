﻿// Copyright (c) Microsoft Corporation.
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

        private bool _RaisedUsingInvariantCulture = false;

        // Pipeline logging
        private string _LogPrefix;
        private int _ObjectNumber;

        private readonly Stopwatch _RuleTimer;
        private readonly List<string> _Reason;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal RunspaceContext(PipelineContext pipeline, PipelineWriter writer)
        {
            Writer = writer;
            CurrentThread = this;
            Pipeline = pipeline;

            _InconclusiveWarning = Pipeline.Option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = Pipeline.Option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;
            _FailStream = Pipeline.Option.Logging.RuleFail ?? LoggingOption.Default.RuleFail.Value;
            _PassStream = Pipeline.Option.Logging.RulePass ?? LoggingOption.Default.RulePass.Value;

            _ObjectNumber = -1;
            _RuleTimer = new Stopwatch();
            _Reason = new List<string>();
        }

        public void Pass()
        {
            if (Writer == null || _PassStream == OutcomeLogStream.None)
                return;

            if (_PassStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName));

            if (_PassStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(new RuleRuntimeException(string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName)), SOURCE_OUTCOME_PASS, ErrorCategory.InvalidData, null));

            if (_PassStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(messageData: string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, Pipeline.Binder.TargetName), source: SOURCE_OUTCOME_PASS));
        }

        public void Fail()
        {
            if (Writer == null || _FailStream == OutcomeLogStream.None)
                return;

            if (_FailStream == OutcomeLogStream.Warning && Writer.ShouldWriteWarning())
                Writer.WriteWarning(string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName));

            if (_FailStream == OutcomeLogStream.Error && Writer.ShouldWriteError())
                Writer.WriteError(new ErrorRecord(new RuleRuntimeException(string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName)), SOURCE_OUTCOME_FAIL, ErrorCategory.InvalidData, null));

            if (_FailStream == OutcomeLogStream.Information && Writer.ShouldWriteInformation())
                Writer.WriteInformation(new InformationRecord(messageData: string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, Pipeline.Binder.TargetName), source: SOURCE_OUTCOME_FAIL));
        }

        public void WarnRuleInconclusive(string ruleId)
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_InconclusiveWarning)
                return;

            Writer.WriteWarning(string.Format(PSRuleResources.RuleInconclusive, ruleId, Pipeline.Binder.TargetName));
        }

        public void WarnObjectNotProcessed()
        {
            if (Writer == null || !Writer.ShouldWriteWarning() || !_NotProcessedWarning)
                return;

            Writer.WriteWarning(string.Format(PSRuleResources.ObjectNotProcessed, Pipeline.Binder.TargetName));
        }

        public void WarnRuleNotFound()
        {
            if (Writer == null || !Writer.ShouldWriteWarning())
                return;

            Writer.WriteWarning(PSRuleResources.RuleNotFound);
        }

        public void ErrorInvaildRuleResult()
        {
            if (Writer == null || !Writer.ShouldWriteError())
                return;

            Writer.WriteError(new ErrorRecord(
                exception: new RuleRuntimeException(message: string.Format(PSRuleResources.InvalidRuleResult, RuleBlock.RuleId)),
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

        public void VerboseFoundRule(string ruleName, string scriptName)
        {
            if (Writer == null || !Writer.ShouldWriteVerbose())
                return;

            Writer.WriteVerbose($"[PSRule][D] -- Found {ruleName} in {scriptName}");
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

            Writer.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- ", string.Format(message, args)));
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
                exception: new RuleParseException(message: error.Message, errorId: error.ErrorId),
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
            if (CurrentThread.Writer == null)
                return;

            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];
            CurrentThread.Error(record);
        }

        public void Error(Exception ex)
        {
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
                category: category
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
                category: error.CategoryInfo.Category
            );
        }

        private string GetStackTrace(ErrorRecord record)
        {
            if (RuleBlock == null)
                return record.ScriptStackTrace;

            return string.Concat(
                record.ScriptStackTrace,
                Environment.NewLine,
                string.Format(PSRuleResources.RuleStackTrace, RuleBlock.RuleName, RuleBlock.Extent.File, RuleBlock.Extent.StartLineNumber)
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

        private string GetLogPrefix()
        {
            if (_LogPrefix == null)
                _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";

            return _LogPrefix ?? string.Empty;
        }

        internal SourceScope EnterSourceScope(SourceFile source)
        {
            if (!File.Exists(source.Path))
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
            Pipeline.Binder.Bind(Pipeline.Baseline, targetObject);
            if (Pipeline.ContentCache.Count > 0)
                Pipeline.ContentCache.Clear();
        }

        /// <summary>
        /// Enter the rule block scope.
        /// </summary>
        public RuleRecord EnterRuleBlock(RuleBlock ruleBlock)
        {
            Pipeline.Binder.Bind(Pipeline.Baseline, TargetObject);
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
            // Do nothing
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
