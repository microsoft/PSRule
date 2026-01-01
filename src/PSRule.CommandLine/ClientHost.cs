// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.CommandLine;

/// <summary>
/// A host context for .NET processes.
/// </summary>
public sealed class ClientHost : HostContext
{
    private readonly ClientContext _Context;
    private readonly bool _Verbose;
    private readonly bool _Debug;
    private readonly ConsoleColor _BackgroundColor;
    private readonly ConsoleColor _ForegroundColor;

    /// <summary>
    /// Create a client host.
    /// </summary>
    /// <param name="context">A client context.</param>
    /// <param name="verbose">Enable or disable verbose log output.</param>
    /// <param name="debug">Enable or disable debug log output.</param>
    public ClientHost(ClientContext context, bool verbose, bool debug)
    {
        _Context = context;
        _Verbose = verbose;
        _Debug = debug;
        _BackgroundColor = System.Console.BackgroundColor;
        _ForegroundColor = System.Console.ForegroundColor;

        _Context.LogVerbose($"[PSRule] -- Using working path: {Directory.GetCurrentDirectory()}");
    }

    /// <summary>
    /// Handles preference variables.
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public override ActionPreference GetPreferenceVariable(string variableName)
    {
        if (variableName == VerbosePreference)
            return _Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        if (variableName == DebugPreference)
            return _Debug ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        return base.GetPreferenceVariable(variableName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public override bool ShouldProcess(string target, string action)
    {
        return true;
    }

    /// <inheritdoc/>
    public override void WriteHost(string message, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null, bool? noNewLine = null)
    {
        System.Console.BackgroundColor = backgroundColor.GetValueOrDefault(_BackgroundColor);
        System.Console.ForegroundColor = foregroundColor.GetValueOrDefault(_ForegroundColor);

        if (noNewLine.GetValueOrDefault(false))
            _Context.Console.Out.Write(message);
        else
            _Context.Console.Out.WriteLine(message);

        RevertConsole();
    }

    private void RevertConsole()
    {
        System.Console.BackgroundColor = _BackgroundColor;
        System.Console.ForegroundColor = _ForegroundColor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string GetWorkingPath()
    {
        return _Context.WorkingPath == null ? base.GetWorkingPath() : _Context.WorkingPath;
    }

    /// <inheritdoc/>
    public override string? CachePath => _Context.CachePath;


    /// <summary>
    /// Determine if a log level is enabled.
    /// All log levels are enabled by default except Trace and Debug.
    /// Trace and Debug are enabled if the verbose or debug arguments are set in the constructor.
    /// </summary>
    public override bool IsEnabled(Runtime.LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return _Verbose;
            case LogLevel.Debug:
                return _Debug;
        }
        return true;
    }

    /// <inheritdoc/>
    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        switch (logLevel)
        {
            case LogLevel.Trace:
                _Context.LogVerbose(formatter(state, exception));
                break;
            case LogLevel.Debug:
                _Context.LogDebug(formatter(state, exception));
                break;
            case LogLevel.Information:
                _Context.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Warning:
                _Context.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                if (exception is PipelineException pipelineException)
                {
                    // If the error is a pipeline exception, set the last error code.
                    _Context.SetLastErrorCode(pipelineException.EventId);
                }

                _Context.LogError(formatter(state, exception));
                base.Log(logLevel, eventId, state, exception, formatter);
                break;
        }
    }

    /// <inheritdoc/>
    public override void SetExitCode(int exitCode)
    {
        if (exitCode == 0) return;

        _Context.SetLastErrorCode(exitCode);
        base.SetExitCode(exitCode);
    }
}
