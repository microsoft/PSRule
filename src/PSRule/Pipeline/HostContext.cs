// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;

namespace PSRule.Pipeline
{
    public interface IHostContext
    {
        bool InSession { get; }

        ActionPreference GetPreferenceVariable(string variableName);

        T GetVariable<T>(string variableName);

        void SetVariable<T>(string variableName, T value);

        void Error(ErrorRecord errorRecord);

        void Warning(string text);

        void Information(InformationRecord informationRecord);

        void Verbose(string text);

        void Debug(string text);

        void Object(object sendToPipeline, bool enumerateCollection);

        bool ShouldProcess(string target, string action);
    }

    internal static class HostContextExtensions
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string InformationPreference = "InformationPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";
        private const string AutoLoadingPreference = "PSModuleAutoLoadingPreference";

        public static ActionPreference GetErrorPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(ErrorPreference);
        }

        public static ActionPreference GetWarningPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(WarningPreference);
        }

        public static ActionPreference GetInformationPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(InformationPreference);
        }

        public static ActionPreference GetVerbosePreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(VerbosePreference);
        }

        public static ActionPreference GetDebugPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(DebugPreference);
        }

        public static PSModuleAutoLoadingPreference GetAutoLoadingPreference(this IHostContext hostContext)
        {
            return hostContext.GetVariable<PSModuleAutoLoadingPreference>(AutoLoadingPreference);
        }
    }

    public abstract class HostContext : IHostContext
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";

        public virtual bool InSession => false;

        public virtual void Debug(string text)
        {

        }

        public virtual void Error(ErrorRecord errorRecord)
        {

        }

        public virtual ActionPreference GetPreferenceVariable(string variableName)
        {
            return variableName == ErrorPreference ||
                variableName == WarningPreference ? ActionPreference.Continue : ActionPreference.Ignore;
        }

        public virtual T GetVariable<T>(string variableName)
        {
            return default;
        }

        public virtual void Information(InformationRecord informationRecord)
        {

        }

        public virtual void Object(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline is IResultRecord record)
                Record(record);
            //else if (enumerateCollection)
            //    foreach (var item in record)
        }

        public virtual void SetVariable<T>(string variableName, T value)
        {

        }

        public abstract bool ShouldProcess(string target, string action);

        public virtual void Verbose(string text)
        {

        }

        public virtual void Warning(string text)
        {

        }

        public virtual void Record(IResultRecord record)
        {

        }
    }

    public sealed class PSHostContext : IHostContext
    {
        internal readonly PSCmdlet CmdletContext;
        internal readonly EngineIntrinsics ExecutionContext;

        /// <summary>
        /// Determine if running in a remote session.
        /// </summary>
        public bool InSession { get; }

        public PSHostContext(PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            InSession = false;
            CmdletContext = commandRuntime;
            ExecutionContext = executionContext;
            InSession = executionContext != null && executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
        }

        public ActionPreference GetPreferenceVariable(string variableName)
        {
            return ExecutionContext == null
                ? ActionPreference.SilentlyContinue
                : (ActionPreference)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
        }

        public T GetVariable<T>(string variableName)
        {
            return ExecutionContext == null ? default : (T)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
        }

        public void SetVariable<T>(string variableName, T value)
        {
            CmdletContext.SessionState.PSVariable.Set(variableName, value);
        }

        public bool ShouldProcess(string target, string action)
        {
            return CmdletContext == null || CmdletContext.ShouldProcess(target, action);
        }

        public void Error(ErrorRecord errorRecord)
        {
            CmdletContext.WriteError(errorRecord);
        }

        public void Warning(string text)
        {
            CmdletContext.WriteWarning(text);
        }

        public void Information(InformationRecord informationRecord)
        {
            CmdletContext.WriteInformation(informationRecord);
        }

        public void Verbose(string text)
        {
            CmdletContext.WriteVerbose(text);
        }

        public void Debug(string text)
        {
            CmdletContext.WriteDebug(text);
        }

        public void Object(object sendToPipeline, bool enumerateCollection)
        {
            CmdletContext.WriteObject(sendToPipeline, enumerateCollection);
        }
    }
}
