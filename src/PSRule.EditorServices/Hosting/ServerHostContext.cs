// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.CommandLine;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.EditorServices.Hosting;

/// <summary>
/// A host context for running PSRule in the language server.
/// </summary>
internal sealed class ServerHostContext : HostContext
{
    private readonly ClientContext _ClientContext;
    private readonly bool _Verbose;
    private readonly bool _Debug;

    public ServerHostContext(ClientContext clientContext, bool verbose, bool debug)
    {
        _ClientContext = clientContext;
        _Verbose = verbose;
        _Debug = debug;

        if (_Verbose)
        {
            _ClientContext.Console.Out.WriteLine($"Using working path: {Directory.GetCurrentDirectory()}");
        }
    }

    public override ActionPreference GetPreferenceVariable(string variableName)
    {
        if (variableName == VerbosePreference)
            return _Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        if (variableName == DebugPreference)
            return _Debug ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        return base.GetPreferenceVariable(variableName);
    }

    public override bool ShouldProcess(string target, string action)
    {
        return true;
    }

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

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        switch (logLevel)
        {
            case LogLevel.Trace:
                _ClientContext.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Debug:
                _ClientContext.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Information:
                _ClientContext.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Warning:
                _ClientContext.Console.Out.WriteLine(formatter(state, exception));
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                _ClientContext.Console.Error.WriteLine(formatter(state, exception));
                base.Log(logLevel, eventId, state, exception, formatter);
                break;
        }
    }
}
