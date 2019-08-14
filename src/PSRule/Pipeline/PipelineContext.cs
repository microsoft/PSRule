using PSRule.Configuration;
using PSRule.Host;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Text;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext : IDisposable, IBindingContext
    {
        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        // Configuration parameters
        private readonly ILogger _Logger;
        private readonly BindTargetName _BindTargetName;
        private readonly BindTargetName _BindTargetType;
        private readonly LanguageMode _LanguageMode;
        private readonly bool _InconclusiveWarning;
        private readonly bool _NotProcessedWarning;
        private readonly OutcomeLogStream _FailStream;
        private readonly OutcomeLogStream _PassStream;
        private readonly Dictionary<string, NameToken> _NameTokenCache;
        private readonly Stopwatch _RuleTimer;

        private readonly List<string> _Reason;

        // Pipeline logging
        private string _LogPrefix;
        private int _ObjectNumber;

        // Objects kept for caching and disposal
        private Runspace _Runspace;
        private SHA1Managed _Hash;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        // Fields exposed to engine
        internal RuleRecord RuleRecord;
        internal string TargetName;
        internal string TargetType;
        internal PSObject TargetObject;
        internal RuleBlock RuleBlock;
        internal PSRuleOption Option;
        internal RuleSource Source;
        internal Dictionary<string, Hashtable> DataCache;
        internal string[] SourceContentCache;
        internal ExecutionScope ExecutionScope;

        public HashAlgorithm ObjectHashAlgorithm
        {
            get
            {
                if (_Hash == null)
                {
                    _Hash = new SHA1Managed();
                }

                return _Hash;
            }
        }

        internal ILogger Logger
        {
            get
            {
                return _Logger;
            }
        }

        private PipelineContext(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, BindTargetName bindTargetType)
        {
            _ObjectNumber = -1;
            _Logger = logger;
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            _RuleTimer = new Stopwatch();

            Option = option;

            _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode.Value;

            _InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;
            _FailStream = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail.Value;
            _PassStream = option.Logging.RulePass ?? LoggingOption.Default.RulePass.Value;

            _NameTokenCache = new Dictionary<string, NameToken>();
            DataCache = new Dictionary<string, Hashtable>();

            _Reason = new List<string>();
        }

        public static PipelineContext New(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, BindTargetName bindTargetType)
        {
            var context = new PipelineContext(logger, option, bindTargetName, bindTargetType);
            CurrentThread = context;
            return context;
        }

        internal Runspace GetRunspace()
        {
            if (_Runspace == null)
            {
                var state = HostState.CreateSessionState();

                // Set PowerShell language mode
                state.LanguageMode = _LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

                _Runspace = RunspaceFactory.CreateRunspace(state);
                _Runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;

                if (Runspace.DefaultRunspace == null)
                {
                    Runspace.DefaultRunspace = _Runspace;
                }

                _Runspace.Open();
                _Runspace.SessionStateProxy.PSVariable.Set(new RuleVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new LocalizedDataVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new AssertVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new ConfigurationVariable());
                _Runspace.SessionStateProxy.PSVariable.Set("ErrorActionPreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set("WarningPreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set("VerbosePreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set("DebugPreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.Path.SetLocation(PSRuleOption.GetWorkingPath());
            }

            return _Runspace;
        }

        internal void EnterSourceScope(RuleSource source)
        {
            if (!File.Exists(source.Path))
            {
                throw new FileNotFoundException(PSRuleResources.ScriptNotFound, source.Path);
            }

            Source = source;
            SourceContentCache = File.ReadAllLines(source.Path, Encoding.UTF8);
        }

        internal void ExitSourceScope()
        {
            Source = null;
            SourceContentCache = null;
        }

        public void Pass()
        {
            if (_PassStream == OutcomeLogStream.None)
            {
                return;
            }

            if (_PassStream == OutcomeLogStream.Warning && _Logger.ShouldWriteWarning())
            {
                _Logger.WriteWarning(string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, TargetName));
            }

            if (_PassStream == OutcomeLogStream.Error && _Logger.ShouldWriteError())
            {
                _Logger.WriteError(new ErrorRecord(new RuleRuntimeException(string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, TargetName)), "Rule.Outcome.Pass", ErrorCategory.InvalidData, null));
            }

            if (_PassStream == OutcomeLogStream.Information && _Logger.ShouldWriteInformation())
            {
                _Logger.WriteInformation(new InformationRecord(messageData: string.Format(PSRuleResources.OutcomeRulePass, RuleRecord.RuleName, TargetName), source: "Rule.Outcome.Pass"));
            }
        }

        public void Fail()
        {
            if (_FailStream == OutcomeLogStream.None)
            {
                return;
            }

            if (_FailStream == OutcomeLogStream.Warning && _Logger.ShouldWriteWarning())
            {
                _Logger.WriteWarning(string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, TargetName));
            }

            if (_FailStream == OutcomeLogStream.Error && _Logger.ShouldWriteError())
            {
                _Logger.WriteError(new ErrorRecord(new RuleRuntimeException(string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, TargetName)), "Rule.Outcome.Fail", ErrorCategory.InvalidData, null));
            }

            if (_FailStream == OutcomeLogStream.Information && _Logger.ShouldWriteInformation())
            {
                _Logger.WriteInformation(new InformationRecord(messageData: string.Format(PSRuleResources.OutcomeRuleFail, RuleRecord.RuleName, TargetName), source: "Rule.Outcome.Fail"));
            }
        }

        public void WarnRuleInconclusive(string ruleId)
        {
            if (!_Logger.ShouldWriteWarning() || !_InconclusiveWarning)
            {
                return;
            }

            _Logger.WriteWarning(string.Format(PSRuleResources.RuleInconclusive, ruleId, TargetName));
        }

        public void WarnObjectNotProcessed()
        {
            if (!_Logger.ShouldWriteWarning() || !_NotProcessedWarning)
            {
                return;
            }

            _Logger.WriteWarning(message: string.Format(PSRuleResources.ObjectNotProcessed, TargetName));
        }

        public void WarnRuleNotFound()
        {
            if (!_Logger.ShouldWriteWarning())
            {
                return;
            }

            _Logger.WriteWarning(message: PSRuleResources.RuleNotFound);
        }

        public void ErrorInvaildRuleResult()
        {
            if (!_Logger.ShouldWriteError())
            {
                return;
            }

            _Logger.WriteError(errorRecord: new ErrorRecord(
                exception: new RuleRuntimeException(message: string.Format(PSRuleResources.InvalidRuleResult, RuleBlock.RuleId)),
                errorId: "PSRule.Runtime.InvalidRuleResult",
                errorCategory: ErrorCategory.InvalidResult,
                targetObject: null
            ));
        }

        public void VerboseRuleDiscovery(string path)
        {
            if (!_Logger.ShouldWriteVerbose() || string.IsNullOrEmpty(path))
            {
                return;
            }

            _Logger.WriteVerbose($"[PSRule][D] -- Discovering rules in: {path}");
        }

        public void VerboseFoundRule(string ruleName, string scriptName)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }


            _Logger.WriteVerbose($"[PSRule][D] -- Found {ruleName} in {scriptName}");
        }

        public void VerboseObjectStart()
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }

            _Logger.WriteVerbose(string.Concat(GetLogPrefix(), " :: ", TargetName));
        }

        public void VerboseConditionMessage(string condition, string message, params object[] args)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }

            _Logger.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- ", string.Format(message, args)));
        }

        public void VerboseConditionResult(string condition, int pass, int count, bool outcome)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }

            _Logger.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- [", pass, "/", count, "] [", outcome, "]"));
        }

        public void VerboseConditionResult(string condition, bool outcome)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }

            _Logger.WriteVerbose(string.Concat(GetLogPrefix(), "[", condition, "] -- [", outcome, "]"));
        }

        public void VerboseConditionResult(int pass, int count, RuleOutcome outcome)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }

            _Logger.WriteVerbose(string.Concat(GetLogPrefix(), " -- [", pass, "/", count, "] [", outcome, "]"));
        }

        public void WriteError(ErrorRecord record)
        {
            if (!_Logger.ShouldWriteError())
            {
                return;
            }
            _Logger.WriteError(errorRecord: record);
        }

        public void WriteError(ParseError error)
        {
            if (!_Logger.ShouldWriteError())
            {
                return;
            }
            var record = new ErrorRecord
            (
                exception: new RuleParseException(message: error.Message, errorId: error.ErrorId),
                errorId: error.ErrorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            );
            _Logger.WriteError(errorRecord: record);
        }

        internal static void EnableLogging(PowerShell ps)
        {
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Warning_DataAdded;
            ps.Streams.Verbose.DataAdded += Verbose_DataAdded;
            ps.Streams.Information.DataAdded += Information_DataAdded;
            ps.Streams.Debug.DataAdded += Debug_DataAdded;
        }

        private static void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<DebugRecord>;
            var record = collection[e.Index];

            CurrentThread._Logger.WriteDebug(debugRecord: record);
        }

        private static void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<InformationRecord>;
            var record = collection[e.Index];

            CurrentThread._Logger.WriteInformation(informationRecord: record);
        }

        private static void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<VerboseRecord>;
            var record = collection[e.Index];

            CurrentThread._Logger.WriteVerbose(record.Message);
        }

        private static void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<WarningRecord>;
            var record = collection[e.Index];

            CurrentThread._Logger.WriteWarning(message: record.Message);
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];

            CurrentThread._Logger.WriteError(errorRecord: record);
        }

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        public void SetTargetObject(PSObject targetObject)
        {
            _ObjectNumber++;

            TargetObject = targetObject;

            // Bind TargetName
            TargetName = _BindTargetName(targetObject);

            // Bind TargetType
            TargetType = _BindTargetType(targetObject);
        }

        /// <summary>
        /// Enter the rule block scope.
        /// </summary>
        public RuleRecord EnterRuleBlock(RuleBlock ruleBlock)
        {
            RuleRecord = new RuleRecord(
                ruleId: ruleBlock.RuleId,
                ruleName: ruleBlock.RuleName,
                targetObject: TargetObject,
                targetName: TargetName,
                targetType: TargetType,
                tag: ruleBlock.Tag,
                info: ruleBlock.Info
            );

            RuleBlock = ruleBlock;

            _Logger.EnterScope(ruleBlock.RuleName);

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
            {
                RuleRecord.Reason = _Reason.ToArray();
            }

            _Logger.ExitScope();

            _LogPrefix = null;
            RuleRecord = null;
            RuleBlock = null;
            _Reason.Clear();
        }

        public void WriteReason(string text)
        {
            if (string.IsNullOrEmpty(text) || ExecutionScope != ExecutionScope.Condition)
            {
                return;
            }

            _Reason.Add(text);
        }

        private string GetLogPrefix()
        {
            if (_LogPrefix == null)
            {
                _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";
            }

            return _LogPrefix ?? string.Empty;
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
                    {
                        _Hash.Dispose();
                    }

                    if (_Runspace != null)
                    {
                        _Runspace.Dispose();
                    }

                    _RuleTimer.Stop();
                    _NameTokenCache.Clear();
                    DataCache.Clear();
                }

                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
