// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal abstract class PipelineLoggerBase : IPipelineWriter
{
    private const string Source = "PSRule";
    private const string HostTag = "PSHOST";

    protected string? ScopeName { get; private set; }

    public bool HadErrors { get; private set; }

    public bool HadFailures { get; private set; }

    public int ExitCode { get; private set; }

    #region Logging

    public void WriteError(ErrorRecord errorRecord)
    {
        HadErrors = true;
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

    public void WriteResult(InvokeResult result)
    {

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

    public virtual void Begin()
    {

    }

    public virtual void End(IPipelineResult result)
    {

    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        // Do nothing, but allow override.
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    protected abstract void DoWriteError(ErrorRecord errorRecord);

    protected abstract void DoWriteVerbose(string message);

    protected abstract void DoWriteWarning(string message);

    protected abstract void DoWriteInformation(InformationRecord informationRecord);

    protected abstract void DoWriteDebug(DebugRecord debugRecord);

    protected abstract void DoWriteObject(object sendToPipeline, bool enumerateCollection);

    public virtual bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                return ShouldWriteDebug();

            case LogLevel.Information:
                return ShouldWriteInformation();

            case LogLevel.Warning:
                return ShouldWriteWarning();

            case LogLevel.Error:
            case LogLevel.Critical:
                return ShouldWriteError();
        }
        return false;
    }

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            HadErrors = true;

        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
        {
            WriteError(new ErrorRecord(exception, eventId.Id.ToString(), ErrorCategory.InvalidOperation, null));
        }
        else if (logLevel == LogLevel.Warning)
        {
            WriteWarning(formatter(state, exception));
        }
        else if (logLevel == LogLevel.Information)
        {
            WriteInformation(new InformationRecord(formatter(state, exception), null));
        }
        else if (logLevel == LogLevel.Debug || logLevel == LogLevel.Trace)
        {
            WriteDebug(formatter(state, exception));
        }
    }

    /// <inheritdoc/>
    public void SetExitCode(int exitCode)
    {
        if (exitCode == 0) return;

        ExitCode = exitCode;
    }
}
