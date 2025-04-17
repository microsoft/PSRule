// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.IO;
using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule.CommandLine;

/// <summary>
/// 
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
        _BackgroundColor = Console.BackgroundColor;
        _ForegroundColor = Console.ForegroundColor;

        Verbose($"[PSRule] -- Using working path: {Directory.GetCurrentDirectory()}");
    }

    /// <summary>
    /// Handles preference variables.
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public override ActionPreference GetPreferenceVariable(string variableName)
    {
        if (variableName == "VerbosePreference")
            return _Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        if (variableName == "DebugPreference")
            return _Debug ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        return base.GetPreferenceVariable(variableName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorRecord"></param>
    public override void Error(ErrorRecord errorRecord)
    {
        if (errorRecord.Exception is PipelineException pipelineException)
        {
            // If the error is a pipeline exception, set the last error code.
            _Context.SetLastErrorCode(pipelineException.EventId);
        }

        _Context.LogError(errorRecord.Exception.Message);
        base.Error(errorRecord);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public override void Warning(string text)
    {
        _Context.Invocation.Console.WriteLine(text);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="informationRecord"></param>
    public override void Information(InformationRecord informationRecord)
    {
        if (informationRecord?.MessageData is HostInformationMessage info)
        {
            SetConsole(info);
            if (info.NoNewLine.GetValueOrDefault(false))
                _Context.Invocation.Console.Write(info.Message);
            else
                _Context.Invocation.Console.WriteLine(info.Message);

            RevertConsole();
        }
    }

    private void SetConsole(HostInformationMessage info)
    {
        Console.BackgroundColor = info.BackgroundColor.GetValueOrDefault(_BackgroundColor);
        Console.ForegroundColor = info.ForegroundColor.GetValueOrDefault(_ForegroundColor);
    }

    private void RevertConsole()
    {
        Console.BackgroundColor = _BackgroundColor;
        Console.ForegroundColor = _ForegroundColor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public override void Verbose(string text)
    {
        if (!_Verbose)
            return;

        _Context.LogVerbose(text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public override void Debug(string text)
    {
        if (!_Debug)
            return;

        _Context.Invocation.Console.WriteLine(text);
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
}
