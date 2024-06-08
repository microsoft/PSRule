// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline;

/// <summary>
/// A base class for writers.
/// </summary>
internal abstract class PipelineWriter(IPipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess) : IPipelineWriter
{
    protected const string ErrorPreference = "ErrorActionPreference";
    protected const string WarningPreference = "WarningPreference";
    protected const string VerbosePreference = "VerbosePreference";
    protected const string InformationPreference = "InformationPreference";
    protected const string DebugPreference = "DebugPreference";

    private readonly IPipelineWriter _Writer = inner;
    private readonly ShouldProcess _ShouldProcess = shouldProcess;

    protected readonly PSRuleOption Option = option;

    private bool _IsDisposed;
    private bool _HadErrors;
    private bool _HadFailures;

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
    public virtual void End(IPipelineResult result)
    {
        if (_Writer == null)
            return;

        _Writer.End(result);
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
    protected static ActionPreference GetPreferenceVariable(SessionState sessionState, string variableName)
    {
        return (ActionPreference)sessionState.PSVariable.GetValue(variableName);
    }
}
