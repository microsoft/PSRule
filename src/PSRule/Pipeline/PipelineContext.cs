using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Management.Automation;
using System.Security.Cryptography;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext : IDisposable
    {
        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        private string _LogPrefix;
        private int _ObjectNumber;
        private readonly ILogger _Logger;
        private readonly BindTargetName _BindTargetName;
        private readonly bool _LogError;
        private readonly bool _LogWarning;
        private readonly bool _LogVerbose;
        private readonly bool _InconclusiveWarning;
        private readonly bool _NotProcessedWarning;
        internal RuleRecord _Rule;

        private SHA1Managed _Hash;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        public string TargetName { get; private set; }

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

        private PipelineContext(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, bool logError, bool logWarning, bool logVerbose)
        {
            _ObjectNumber = -1;
            _Logger = logger;
            _BindTargetName = bindTargetName;
            _LogError = logError;
            _LogWarning = logWarning;
            _LogVerbose = logVerbose;

            _InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning.Value;
            _NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning.Value;

            if (_Logger == null)
            {
                _LogError = _LogWarning = _LogVerbose = false;
            }
        }

        public static PipelineContext New(ILogger logger, PSRuleOption option, BindTargetName bindTargetName, bool logError = true, bool logWarning = true, bool logVerbose = false)
        {
            var context = new PipelineContext(logger, option, bindTargetName, logError, logWarning, logVerbose);
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

        public void WriteVerboseObjectStart()
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($" :: {TargetName}", usePrefix: true);
        }

        public void WriteVerboseConditionResult(string condition, int pass, int count, bool outcome)
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($"{condition} -- [{pass}/{count}] [{outcome}]", usePrefix: true);
        }

        public void WriteVerboseConditionResult(int? pass, int? count, RuleOutcome outcome)
        {
            if (!_LogVerbose)
            {
                return;
            }

            DoWriteVerbose($" -- [{pass}/{count}] [{outcome}]", usePrefix: true);
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
            var outMessage = usePrefix ? string.Concat(_LogPrefix, message) : message;
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

        #endregion Internal logging methods

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        public void TargetObject(PSObject targetObject)
        {
            _ObjectNumber++;

            // Bind targetname
            TargetName = _BindTargetName(targetObject);
        }

        /// <summary>
        /// Enter the rule block scope.
        /// </summary>
        public void Enter(RuleBlock ruleBlock)
        {
            _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{ruleBlock.RuleId}]";
        }

        /// <summary>
        /// Exit the rule block scope.
        /// </summary>
        public void Exit()
        {
            _LogPrefix = string.Empty;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            // Already cleaned up by dispose.
            GC.SuppressFinalize(this);
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
                }

                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
