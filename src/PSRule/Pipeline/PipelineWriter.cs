// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A base class for pipeline writers that passes through to an inner writer.
/// </summary>
internal abstract class PipelineWriter(IPipelineWriter? inner, PSRuleOption option, ShouldProcess shouldProcess) : IPipelineWriter
{
    protected const string ErrorPreference = "ErrorActionPreference";
    protected const string WarningPreference = "WarningPreference";
    protected const string VerbosePreference = "VerbosePreference";
    protected const string InformationPreference = "InformationPreference";
    protected const string DebugPreference = "DebugPreference";

    private readonly IPipelineWriter? _Inner = inner;
    private readonly ShouldProcess _ShouldProcess = shouldProcess;
    protected readonly PSRuleOption Option = option;

    private bool _IsDisposed;

    /// <inheritdoc/>
    public int ExitCode => _Inner?.ExitCode ?? 0;

    /// <inheritdoc/>
    public virtual bool HadErrors => _Inner?.HadErrors ?? false;

    /// <inheritdoc/>
    public virtual bool HadFailures => _Inner?.HadFailures ?? false;

    /// <inheritdoc/>
    public virtual void Begin()
    {
        _Inner?.Begin();
    }

    /// <inheritdoc/>
    public virtual void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        _Inner?.WriteObject(sendToPipeline, enumerateCollection);
    }

    /// <inheritdoc/>
    public virtual void WriteResult(InvokeResult result)
    {
        _Inner?.WriteResult(result);
    }

    /// <inheritdoc/>
    public virtual void End(IPipelineResult result)
    {
        _Inner?.End(result);
    }

    /// <inheritdoc/>
    public virtual void WriteHost(HostInformationMessage info)
    {
        _Inner?.WriteHost(info);
    }

    /// <inheritdoc/>
    public virtual void SetExitCode(int exitCode)
    {
        _Inner?.SetExitCode(exitCode);
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_IsDisposed)
        {
            if (disposing && _Inner != null)
                _Inner.Dispose();

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
        _Inner?.LogError(errorRecord);
    }

    protected bool CreateFile(string path)
    {
        return CreatePath(path) && ShouldProcess(target: path, action: PSRuleResources.ShouldWriteFile);
    }

    #region ILogger

    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return _Inner?.IsEnabled(logLevel) ?? false;
    }

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _Inner?.Log(logLevel, eventId, state, exception, formatter);
    }

    #endregion ILogger

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
}
