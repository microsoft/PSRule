using PSRule.Configuration;
using PSRule.Host;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext : IDisposable
    {
        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        // Configuration parameters
        private readonly ILogger _Logger;
        private readonly BindTargetName _BindTargetName;
        private readonly bool _LogError;
        private readonly bool _LogWarning;
        private readonly bool _LogVerbose;
        private readonly bool _LogInformation;
        private readonly LanguageMode _LanguageMode;
        private readonly bool _InconclusiveWarning;
        private readonly bool _NotProcessedWarning;

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
        internal PSObject TargetObject;
        internal RuleBlock RuleBlock;
        internal PSRuleOption Option;

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

        private PipelineContext(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, bool logError, bool logWarning, bool logVerbose, bool logInformation)
        {
            _ObjectNumber = -1;
            _Logger = logger;
            _BindTargetName = bindTargetName;
            _LogError = logError;
            _LogWarning = logWarning;
            _LogVerbose = logVerbose;
            _LogInformation = logInformation;

            Option = option;

            _LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode.Value;

            _InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;

            if (_Logger == null)
            {
                _LogError = _LogWarning = _LogVerbose = _LogInformation = false;
            }
        }

        public static PipelineContext New(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, bool logError = true, bool logWarning = true, bool logVerbose = false, bool logInformation = false)
        {
            var context = new PipelineContext(logger, option, bindTargetName, logError, logWarning, logVerbose, logInformation);
            CurrentThread = context;
            return context;
        }

        #region Logging

        public void WriteError(ErrorRecord errorRecord)
        {
            if (!_LogError || errorRecord == null)
            {
                return;
            }

            DoWriteError(errorRecord);
        }

        public void WriteVerbose(string message, bool usePrefix = true)
        {
            if (!_LogVerbose || string.IsNullOrEmpty(message))
            {
                return;
            }

            DoWriteVerbose(message, usePrefix);
        }

        public void VerboseRuleDiscovery(string path)
        {
            if (!_LogVerbose || string.IsNullOrEmpty(path))
            {
                return;
            }

            DoWriteVerbose($"[PSRule][D] -- Discovering rules in: {path}", usePrefix: false);
        }

        public void VerboseFoundRule(string ruleName, string scriptName)
        {
            if (!_LogVerbose)
            {
                return;
            }


            DoWriteVerbose($"[PSRule][D] -- Found {ruleName} in {scriptName}", usePrefix: false);
        }

        public void VerboseObjectStart()
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($" :: {TargetName}", usePrefix: true);
        }

        public void VerboseConditionResult(string condition, int pass, int count, bool outcome)
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($"[{condition}] -- [{pass}/{count}] [{outcome}]", usePrefix: true);
        }

        public void VerboseConditionResult(string condition, bool outcome)
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($"[{condition}] -- [{outcome}]", usePrefix: true);
        }

        public void VerboseConditionResult(int pass, int count, RuleOutcome outcome)
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($" -- [{pass}/{count}] [{outcome}]", usePrefix: true);
        }

        public void WriteInformation(InformationRecord informationRecord)
        {
            if (!_LogInformation)
            {
                return;
            }

            DoWriteInformation(informationRecord);
        }

        public void WriteWarning(string message)
        {
            if (!_LogWarning)
            {
                return;
            }

            DoWriteWarning(message);
        }

        public void WarnRuleInconclusive(string ruleId)
        {
            if (!_LogWarning || !_InconclusiveWarning)
            {
                return;
            }

            DoWriteWarning(string.Format(PSRuleResources.RuleInconclusive, ruleId, TargetName));
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
                _Runspace.SessionStateProxy.PSVariable.Set(new RuleVariable("Rule"));
                _Runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable("TargetObject"));
                _Runspace.SessionStateProxy.PSVariable.Set("ErrorActionPreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set("WarningPreference", ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set("VerbosePreference", ActionPreference.Continue);
            }

            return _Runspace;
        }

        public void WarnObjectNotProcessed()
        {
            if (!_LogWarning || !_NotProcessedWarning)
            {
                return;
            }

            DoWriteWarning(string.Format(PSRuleResources.ObjectNotProcessed, TargetName));
        }

        public void WarnRuleNotFound()
        {
            if (!_LogWarning)
            {
                return;
            }

            DoWriteWarning(PSRuleResources.RuleNotFound);
        }

        #endregion Logging

        #region Internal logging methods

        /// <summary>
        /// Core methods to hand off to logger.
        /// </summary>
        /// <param name="errorRecord">A valid PowerShell error record.</param>
        private void DoWriteError(ErrorRecord errorRecord)
        {
            _Logger.WriteError(errorRecord);
        }

        /// <summary>
        /// Core method to hand off verbose messages to logger.
        /// </summary>
        /// <param name="message">A message to log.</param>
        /// <param name="usePrefix">When true a prefix indicating the current rule and target object will prefix the message.</param>
        private void DoWriteVerbose(string message, bool usePrefix)
        {
            var outMessage = usePrefix ? string.Concat(GetLogPrefix(), message) : message;
            _Logger.WriteVerbose(outMessage);
        }

        /// <summary>
        /// Core methods to hand off to logger.
        /// </summary>
        /// <param name="message">A message to log</param>
        private void DoWriteWarning(string message)
        {
            _Logger.WriteWarning(message);
        }

        private void DoWriteInformation(InformationRecord informationRecord)
        {
            _Logger.WriteInformation(informationRecord);
        }

        #endregion Internal logging methods

        internal static void EnableLogging(PowerShell ps)
        {
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Warning_DataAdded;
            ps.Streams.Verbose.DataAdded += Verbose_DataAdded;
            ps.Streams.Information.DataAdded += Information_DataAdded;
        }

        private static void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<InformationRecord>;
            var record = collection[e.Index];

            CurrentThread.WriteInformation(informationRecord: record);
        }

        private static void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<VerboseRecord>;
            var record = collection[e.Index];

            CurrentThread.WriteVerbose(record.Message, usePrefix: false);
        }

        private static void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<WarningRecord>;
            var record = collection[e.Index];

            CurrentThread.WriteWarning(message: record.Message);
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];

            CurrentThread.WriteError(errorRecord: record);
        }

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        public void SetTargetObject(PSObject targetObject)
        {
            _ObjectNumber++;

            TargetObject = targetObject;

            // Bind targetname
            TargetName = _BindTargetName(targetObject);
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
                tag: ruleBlock.Tag,
                message: ruleBlock.Description
            );

            RuleBlock = ruleBlock;

            return RuleRecord;
        }

        /// <summary>
        /// Exit the rule block scope.
        /// </summary>
        public void ExitRuleBlock()
        {
            _LogPrefix = null;
            RuleRecord = null;
            RuleBlock = null;
        }

        private string GetLogPrefix()
        {
            if (_LogPrefix == null)
            {
                _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{RuleRecord?.RuleId}]";
            }

            return _LogPrefix ?? string.Empty;
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
                    if (_Hash != null)
                    {
                        _Hash.Dispose();
                    }

                    if (_Runspace != null)
                    {
                        _Runspace.Dispose();
                    }
                }

                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
