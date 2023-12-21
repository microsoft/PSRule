// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule.Tool;

internal sealed class ClientHost : HostContext
{
    private readonly InvocationContext _Invocation;
    private readonly bool _Verbose;
    private readonly bool _Debug;
    private readonly ConsoleColor _BackgroundColor;
    private readonly ConsoleColor _ForegroundColor;

    public ClientHost(InvocationContext invocation, bool verbose, bool debug)
    {
        _Invocation = invocation;
        _Verbose = verbose;
        _Debug = debug;
        _BackgroundColor = Console.BackgroundColor;
        _ForegroundColor = Console.ForegroundColor;

        Verbose($"Using working path: {Directory.GetCurrentDirectory()}");
    }

    public override ActionPreference GetPreferenceVariable(string variableName)
    {
        if (variableName == "VerbosePreference")
            return _Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        if (variableName == "DebugPreference")
            return _Debug ? ActionPreference.Continue : ActionPreference.SilentlyContinue;

        return base.GetPreferenceVariable(variableName);
    }

    public override void Error(ErrorRecord errorRecord)
    {
        _Invocation.Console.Error.WriteLine(errorRecord.Exception.Message);
        base.Error(errorRecord);
    }

    public override void Warning(string text)
    {
        _Invocation.Console.WriteLine(text);
    }

    public override bool ShouldProcess(string target, string action)
    {
        return true;
    }

    public override void Information(InformationRecord informationRecord)
    {
        if (informationRecord?.MessageData is HostInformationMessage info)
        {
            SetConsole(info);
            if (info.NoNewLine.GetValueOrDefault(false))
                _Invocation.Console.Write(info.Message);
            else
                _Invocation.Console.WriteLine(info.Message);

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

    public override void Verbose(string text)
    {
        if (!_Verbose)
            return;

        _Invocation.Console.WriteLine(text);
    }

    public override void Debug(string text)
    {
        if (!_Debug)
            return;

        _Invocation.Console.WriteLine(text);
    }
}
