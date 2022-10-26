// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    /// <summary>
    /// An writer which recieves output from PSRule.
    /// </summary>
    public interface IPipelineWriter
    {
        /// <summary>
        /// Write a verbose message.
        /// </summary>
        void WriteVerbose(string message);

        /// <summary>
        /// Determines if a verbose message should be written to output.
        /// </summary>
        bool ShouldWriteVerbose();

        /// <summary>
        /// Write a warning message.
        /// </summary>
        void WriteWarning(string message);

        /// <summary>
        /// Determines if a warning message should be written to output.
        /// </summary>
        bool ShouldWriteWarning();

        /// <summary>
        /// Write an error message.
        /// </summary>
        void WriteError(ErrorRecord errorRecord);

        /// <summary>
        /// Determines if an error message should be written to output.
        /// </summary>
        bool ShouldWriteError();

        /// <summary>
        /// Write an informational message.
        /// </summary>
        void WriteInformation(InformationRecord informationRecord);

        /// <summary>
        /// Write a message to the host process.
        /// </summary>
        void WriteHost(HostInformationMessage info);

        /// <summary>
        /// Determines if an informational message should be written to output.
        /// </summary>
        bool ShouldWriteInformation();

        /// <summary>
        /// Write a debug message.
        /// </summary>
        void WriteDebug(string text, params object[] args);

        /// <summary>
        /// Determines if a debug message should be written to output.
        /// </summary>
        bool ShouldWriteDebug();

        /// <summary>
        /// Write an object to output.
        /// </summary>
        /// <param name="sendToPipeline">The object to write to the pipeline.</param>
        /// <param name="enumerateCollection">Determines when the object is enumerable if it should be enumerated as more then one object.</param>
        void WriteObject(object sendToPipeline, bool enumerateCollection);

        /// <summary>
        /// Enter a logging scope.
        /// </summary>
        void EnterScope(string scopeName);

        /// <summary>
        /// Exit a logging scope.
        /// </summary>
        void ExitScope();

        /// <summary>
        /// Start and initialize the writer.
        /// </summary>
        void Begin();

        /// <summary>
        /// Stop and finalized the writer.
        /// </summary>
        void End();
    }

    /// <summary>
    /// A base class for writers.
    /// </summary>
    internal abstract class PipelineWriter : IPipelineWriter
    {
        protected const string ErrorPreference = "ErrorActionPreference";
        protected const string WarningPreference = "WarningPreference";
        protected const string VerbosePreference = "VerbosePreference";
        protected const string InformationPreference = "InformationPreference";
        protected const string DebugPreference = "DebugPreference";

        private readonly IPipelineWriter _Writer;

        protected readonly PSRuleOption Option;

        protected PipelineWriter(IPipelineWriter inner, PSRuleOption option)
        {
            _Writer = inner;
            Option = option;
        }

        /// <inheritdoc/>
        public virtual void Begin()
        {
            if (_Writer == null)
                return;

            _Writer.Begin();
        }

        /// <inheritdoc/>
        public virtual void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (_Writer == null || sendToPipeline == null)
                return;

            _Writer.WriteObject(sendToPipeline, enumerateCollection);
        }

        /// <inheritdoc/>
        public virtual void End()
        {
            if (_Writer == null)
                return;

            _Writer.End();
        }

        /// <inheritdoc/>
        public virtual void WriteVerbose(string message)
        {
            if (_Writer == null || string.IsNullOrEmpty(message))
                return;

            _Writer.WriteVerbose(message);
        }

        /// <inheritdoc/>
        public virtual bool ShouldWriteVerbose()
        {
            return _Writer != null && _Writer.ShouldWriteVerbose();
        }

        /// <inheritdoc/>
        public virtual void WriteWarning(string message)
        {
            if (_Writer == null || string.IsNullOrEmpty(message))
                return;

            _Writer.WriteWarning(message);
        }

        /// <inheritdoc/>
        public virtual bool ShouldWriteWarning()
        {
            return _Writer != null && _Writer.ShouldWriteWarning();
        }

        /// <inheritdoc/>
        public virtual void WriteError(ErrorRecord errorRecord)
        {
            if (_Writer == null || errorRecord == null)
                return;

            _Writer.WriteError(errorRecord);
        }

        /// <inheritdoc/>
        public virtual bool ShouldWriteError()
        {
            return _Writer != null && _Writer.ShouldWriteError();
        }

        /// <inheritdoc/>
        public virtual void WriteInformation(InformationRecord informationRecord)
        {
            if (_Writer == null || informationRecord == null)
                return;

            _Writer.WriteInformation(informationRecord);
        }

        /// <inheritdoc/>
        public virtual void WriteHost(HostInformationMessage info)
        {
            if (_Writer == null)
                return;

            _Writer.WriteHost(info);
        }

        /// <inheritdoc/>
        public virtual bool ShouldWriteInformation()
        {
            return _Writer != null && _Writer.ShouldWriteInformation();
        }

        /// <inheritdoc/>
        public virtual void WriteDebug(string text, params object[] args)
        {
            if (_Writer == null || string.IsNullOrEmpty(text) || !ShouldWriteDebug())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            _Writer.WriteDebug(text);
        }

        /// <inheritdoc/>
        public virtual bool ShouldWriteDebug()
        {
            return _Writer != null && _Writer.ShouldWriteDebug();
        }

        /// <inheritdoc/>
        public virtual void EnterScope(string scopeName)
        {
            if (_Writer == null)
                return;

            _Writer.EnterScope(scopeName);
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Get the value of a preference variable.
        /// </summary>
        protected static ActionPreference GetPreferenceVariable(System.Management.Automation.SessionState sessionState, string variableName)
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
