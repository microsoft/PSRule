// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline;

/// <summary>
/// An writer which recieves output from PSRule.
/// </summary>
public interface IPipelineWriter : IDisposable
{
    /// <summary>
    /// Determines if any errors were reported.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// Determines if an failures were reported.
    /// </summary>
    bool HadFailures { get; }

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
    private readonly ShouldProcess _ShouldProcess;

    protected readonly PSRuleOption Option;

    private bool _IsDisposed;
    private bool _HadErrors;
    private bool _HadFailures;

    protected PipelineWriter(IPipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
    {
        _Writer = inner;
        _ShouldProcess = shouldProcess;
        Option = option;
    }

    bool IPipelineWriter.HadErrors => HadErrors;

    bool IPipelineWriter.HadFailures => HadFailures;

    /// <inheritdoc/>
    public virtual bool HadErrors
    {
        get
        {
            return _HadErrors || (_Writer != null && _Writer.HadErrors);
        }
        set
        {
            _HadErrors = value;
        }
    }

    /// <inheritdoc/>
    public virtual bool HadFailures
    {
        get
        {
            return _HadFailures || (_Writer != null && _Writer.HadFailures);
        }
        set
        {
            _HadFailures = value;
        }
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

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_IsDisposed)
        {
            if (disposing && _Writer != null)
                _Writer.Dispose();

            _IsDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

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

    private bool ShouldProcess(string target, string action)
    {
        return _ShouldProcess == null || _ShouldProcess(target, action);
    }

    private bool CreatePath(string path)
    {
        var parentPath = Directory.GetParent(path);
        if (!parentPath.Exists && ShouldProcess(target: parentPath.FullName, action: PSRuleResources.ShouldCreatePath))
        {
            Directory.CreateDirectory(path: parentPath.FullName);
            return true;
        }
        return parentPath.Exists;
    }

    protected bool CreateFile(string path)
    {
        return CreatePath(path) && ShouldProcess(target: path, action: PSRuleResources.ShouldWriteFile);
    }

    /// <summary>
    /// Get the value of a preference variable.
    /// </summary>
    protected static ActionPreference GetPreferenceVariable(System.Management.Automation.SessionState sessionState, string variableName)
    {
        return (ActionPreference)sessionState.PSVariable.GetValue(variableName);
    }
}

internal abstract class ResultOutputWriter<T> : PipelineWriter
{
    private readonly List<T> _Result;

    protected ResultOutputWriter(IPipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
        : base(inner, option, shouldProcess)
    {
        _Result = new List<T>();
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary)
        {
            base.WriteObject(sendToPipeline, enumerateCollection);
            return;
        }

        if (sendToPipeline is InvokeResult result)
        {
            Add(typeof(T) == typeof(RuleRecord) ? result.AsRecord() : result);
        }
        else
        {
            Add(sendToPipeline);
        }
        base.WriteObject(sendToPipeline, enumerateCollection);
    }

    protected void Add(object o)
    {
        if (o is T[] collection)
            _Result.AddRange(collection);
        else if (o is T item)
            _Result.Add(item);
    }

    /// <summary>
    /// Clear any buffers from the writer.
    /// </summary>
    protected virtual void Flush() { }

    protected T[] GetResults()
    {
        return _Result.ToArray();
    }
}

internal abstract class SerializationOutputWriter<T> : ResultOutputWriter<T>
{
    protected SerializationOutputWriter(IPipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
        : base(inner, option, shouldProcess) { }

    public sealed override void End()
    {
        var results = GetResults();
        base.WriteObject(Serialize(results), false);
        ProcessError(results);
        Flush();
        base.End();
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary)
        {
            base.WriteObject(sendToPipeline, enumerateCollection);
            return;
        }

        if (sendToPipeline is InvokeResult result)
        {
            Add(result.AsRecord());
            return;
        }
        Add(sendToPipeline);
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
