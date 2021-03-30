// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    internal abstract class PipelineLoggerBase : IPipelineWriter
    {
        private const string Source = "PSRule";
        private const string HostTag = "PSHOST";

        protected string ScopeName { get; private set; }

        #region Logging

        public void WriteError(ErrorRecord errorRecord)
        {
            if (!ShouldWriteError() || errorRecord == null)
                return;

            DoWriteError(errorRecord);
        }

        public void WriteVerbose(string message)
        {
            if (!ShouldWriteVerbose() || string.IsNullOrEmpty(message))
                return;

            DoWriteVerbose(message);
        }

        public void WriteDebug(DebugRecord debugRecord)
        {
            if (!ShouldWriteDebug())
                return;

            DoWriteDebug(debugRecord);
        }

        public void WriteDebug(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text) || !ShouldWriteDebug())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            DoWriteDebug(new DebugRecord(text));
        }

        public void WriteInformation(InformationRecord informationRecord)
        {
            if (!ShouldWriteInformation())
                return;

            DoWriteInformation(informationRecord);
        }

        public void WriteHost(HostInformationMessage info)
        {
            var record = new InformationRecord(info, Source);
            record.Tags.Add(HostTag);
            DoWriteInformation(record);
        }

        public void WriteWarning(string message)
        {
            if (!ShouldWriteWarning())
                return;

            DoWriteWarning(message);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            DoWriteObject(sendToPipeline, enumerateCollection);
        }

        public void EnterScope(string scopeName)
        {
            ScopeName = scopeName;
        }

        public void ExitScope()
        {
            ScopeName = null;
        }

        #endregion Logging

        public virtual bool ShouldWriteError()
        {
            return true;
        }

        public virtual bool ShouldWriteWarning()
        {
            return true;
        }

        public virtual bool ShouldWriteVerbose()
        {
            return true;
        }

        public virtual bool ShouldWriteInformation()
        {
            return true;
        }

        public virtual bool ShouldWriteDebug()
        {
            return true;
        }

        protected abstract void DoWriteError(ErrorRecord errorRecord);

        protected abstract void DoWriteVerbose(string message);

        protected abstract void DoWriteWarning(string message);

        protected abstract void DoWriteInformation(InformationRecord informationRecord);

        protected abstract void DoWriteDebug(DebugRecord debugRecord);

        protected abstract void DoWriteObject(object sendToPipeline, bool enumerateCollection);
    }

    internal sealed class PipelineLogger : PipelineLoggerBase
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string InformationPreference = "InformationPreference";
        private const string DebugPreference = "DebugPreference";

        private HashSet<string> _VerboseFilter;
        private HashSet<string> _DebugFilter;

        private Action<string> OnWriteWarning;
        private Action<string> OnWriteVerbose;
        private Action<ErrorRecord> OnWriteError;
        private Action<InformationRecord> OnWriteInformation;
        private Action<string> OnWriteDebug;
        internal Action<object, bool> OnWriteObject;

        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;
        private bool _LogDebug;

        internal void UseCommandRuntime(PSCmdlet commandRuntime)
        {
            OnWriteVerbose = commandRuntime.WriteVerbose;
            OnWriteWarning = commandRuntime.WriteWarning;
            OnWriteError = commandRuntime.WriteError;
            OnWriteInformation = commandRuntime.WriteInformation;
            OnWriteDebug = commandRuntime.WriteDebug;
            OnWriteObject = commandRuntime.WriteObject;
        }

        internal void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _LogError = GetPreferenceVariable(executionContext, ErrorPreference);
            _LogWarning = GetPreferenceVariable(executionContext, WarningPreference);
            _LogVerbose = GetPreferenceVariable(executionContext, VerbosePreference);
            _LogInformation = GetPreferenceVariable(executionContext, InformationPreference);
            _LogDebug = GetPreferenceVariable(executionContext, DebugPreference);
        }

        internal void Configure(PSRuleOption option)
        {
            if (option.Logging.LimitVerbose != null && option.Logging.LimitVerbose.Length > 0)
                _VerboseFilter = new HashSet<string>(option.Logging.LimitVerbose);

            if (option.Logging.LimitDebug != null && option.Logging.LimitDebug.Length > 0)
                _DebugFilter = new HashSet<string>(option.Logging.LimitDebug);
        }

        private static bool GetPreferenceVariable(EngineIntrinsics executionContext, string variableName)
        {
            var preference = (ActionPreference)executionContext.SessionState.PSVariable.GetValue(variableName);
            if (preference == ActionPreference.Ignore)
                return false;

            return !(preference == ActionPreference.SilentlyContinue && (variableName == VerbosePreference || variableName == DebugPreference));
        }

        #region Internal logging methods

        /// <summary>
        /// Core methods to hand off to logger.
        /// </summary>
        /// <param name="errorRecord">A valid PowerShell error record.</param>
        protected override void DoWriteError(ErrorRecord errorRecord)
        {
            if (OnWriteError == null)
                return;

            OnWriteError(errorRecord);
        }

        /// <summary>
        /// Core method to hand off verbose messages to logger.
        /// </summary>
        /// <param name="message">A message to log.</param>
        protected override void DoWriteVerbose(string message)
        {
            if (OnWriteVerbose == null)
                return;

            OnWriteVerbose(message);
        }

        /// <summary>
        /// Core method to hand off warning messages to logger.
        /// </summary>
        /// <param name="message">A message to log</param>
        protected override void DoWriteWarning(string message)
        {
            if (OnWriteWarning == null)
                return;

            OnWriteWarning(message);
        }

        /// <summary>
        /// Core method to hand off information messages to logger.
        /// </summary>
        protected override void DoWriteInformation(InformationRecord informationRecord)
        {
            if (OnWriteInformation == null)
                return;

            OnWriteInformation(informationRecord);
        }

        /// <summary>
        /// Core method to hand off debug messages to logger.
        /// </summary>
        protected override void DoWriteDebug(DebugRecord debugRecord)
        {
            if (OnWriteDebug == null)
                return;

            OnWriteDebug(debugRecord.Message);
        }

        protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (OnWriteObject == null)
                return;

            OnWriteObject(sendToPipeline, enumerateCollection);
        }

        #endregion Internal logging methods

        public override bool ShouldWriteVerbose()
        {
            return _LogVerbose && (_VerboseFilter == null || ScopeName == null || _VerboseFilter.Contains(ScopeName));
        }

        public override bool ShouldWriteInformation()
        {
            return true;
        }

        public override bool ShouldWriteDebug()
        {
            return _LogDebug && (_DebugFilter == null || ScopeName == null || _DebugFilter.Contains(ScopeName));
        }
    }
}
