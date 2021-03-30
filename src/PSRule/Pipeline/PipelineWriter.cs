// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    public delegate void MessageHook(string message);

    public delegate void ErrorRecordHook(ErrorRecord errorRecord);

    public interface IPipelineWriter
    {
        void WriteVerbose(string message);

        bool ShouldWriteVerbose();

        void WriteWarning(string message);

        bool ShouldWriteWarning();

        void WriteError(ErrorRecord errorRecord);

        bool ShouldWriteError();

        void WriteInformation(InformationRecord informationRecord);

        void WriteHost(HostInformationMessage info);

        bool ShouldWriteInformation();

        void WriteDebug(DebugRecord debugRecord);

        void WriteDebug(string text, params object[] args);

        bool ShouldWriteDebug();

        void WriteObject(object sendToPipeline, bool enumerateCollection);

        void EnterScope(string scopeName);

        void ExitScope();
    }

    internal abstract class PipelineWriter : IPipelineWriter
    {
        protected const string ErrorPreference = "ErrorActionPreference";
        protected const string WarningPreference = "WarningPreference";
        protected const string VerbosePreference = "VerbosePreference";
        protected const string InformationPreference = "InformationPreference";
        protected const string DebugPreference = "DebugPreference";

        private readonly PipelineWriter _Writer;

        protected readonly PSRuleOption Option;

        protected PipelineWriter(PipelineWriter inner, PSRuleOption option)
        {
            _Writer = inner;
            Option = option;
        }

        public virtual void Begin()
        {
            if (_Writer == null)
                return;

            _Writer.Begin();
        }

        public virtual void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (_Writer == null || sendToPipeline == null)
                return;

            _Writer.WriteObject(sendToPipeline, enumerateCollection);
        }

        public virtual void End()
        {
            if (_Writer == null)
                return;

            _Writer.End();
        }

        public virtual void WriteVerbose(string message)
        {
            if (_Writer == null || string.IsNullOrEmpty(message))
                return;

            _Writer.WriteVerbose(message);
        }

        public virtual bool ShouldWriteVerbose()
        {
            return _Writer != null && _Writer.ShouldWriteVerbose();
        }

        public virtual void WriteWarning(string message)
        {
            if (_Writer == null || string.IsNullOrEmpty(message))
                return;

            _Writer.WriteWarning(message);
        }

        public virtual bool ShouldWriteWarning()
        {
            return _Writer != null && _Writer.ShouldWriteWarning();
        }

        public virtual void WriteError(ErrorRecord errorRecord)
        {
            if (_Writer == null || errorRecord == null)
                return;

            _Writer.WriteError(errorRecord);
        }

        public virtual bool ShouldWriteError()
        {
            return _Writer != null && _Writer.ShouldWriteError();
        }

        public virtual void WriteInformation(InformationRecord informationRecord)
        {
            if (_Writer == null || informationRecord == null)
                return;

            _Writer.WriteInformation(informationRecord);
        }

        public virtual void WriteHost(HostInformationMessage info)
        {
            if (_Writer == null)
                return;

            _Writer.WriteHost(info);
        }

        public virtual bool ShouldWriteInformation()
        {
            return _Writer != null && _Writer.ShouldWriteInformation();
        }

        public virtual void WriteDebug(DebugRecord debugRecord)
        {
            if (_Writer == null || debugRecord == null)
                return;

            _Writer.WriteDebug(debugRecord);
        }

        public void WriteDebug(string text, params object[] args)
        {
            if (_Writer == null || string.IsNullOrEmpty(text) || !ShouldWriteDebug())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            _Writer.WriteDebug(new DebugRecord(text));
        }

        public virtual bool ShouldWriteDebug()
        {
            return _Writer != null && _Writer.ShouldWriteDebug();
        }

        public virtual void EnterScope(string scopeName)
        {
            if (_Writer == null)
                return;

            _Writer.EnterScope(scopeName);
        }

        public virtual void ExitScope()
        {
            if (_Writer == null)
                return;

            _Writer.ExitScope();
        }

        protected void WriteErrorInfo(RuleRecord record)
        {
            if (record == null || record.Error == null)
                return;

            var errorRecord = new ErrorRecord(
                record.Error.Exception,
                record.Error.ErrorId,
                record.Error.Category,
                record.TargetName
            );
            errorRecord.CategoryInfo.TargetType = record.TargetType;
            errorRecord.ErrorDetails = new ErrorDetails(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.ErrorDetailMessage,
                record.RuleId,
                record.Error.Message,
                record.Error.ScriptExtent.File,
                record.Error.ScriptExtent.StartLineNumber,
                record.Error.ScriptExtent.StartColumnNumber
            ));
            WriteError(errorRecord);
        }

        protected static ActionPreference GetPreferenceVariable(SessionState sessionState, string variableName)
        {
            return (ActionPreference)sessionState.PSVariable.GetValue(variableName);
        }
    }

    internal abstract class SerializationOutputWriter<T> : PipelineWriter
    {
        private readonly List<T> _Result;

        protected SerializationOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option)
        {
            _Result = new List<T>();
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary)
                return;

            if (sendToPipeline is InvokeResult result)
            {
                Add(result.AsRecord());
                return;
            }
            Add(sendToPipeline);
        }

        protected void Add(object o)
        {
            if (o is T[] collection)
                _Result.AddRange(collection);
            else if (o is T item)
                _Result.Add(item);
        }

        public sealed override void End()
        {
            var results = _Result.ToArray();
            base.WriteObject(Serialize(results), false);
            ProcessError(results);
        }

        protected abstract string Serialize(T[] o);

        private void ProcessError(T[] results)
        {
            for (var i = 0; i < results.Length; i++)
            {
                if (results[i] is RuleRecord record)
                    WriteErrorInfo(record);
            }
        }
    }
}
