// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Management.Automation;
using PSRule.Tool.Adapters;

namespace PSRule.Tool;

static class Program
{
    // Timeout in minutes to wait for the debugger to attach.
    private const int DEBUGGER_ATTACH_TIMEOUT = 5;

    // Time in milliseconds to wait between debugger attach checks.
    private const int DEBUGGER_ATTACH_CYCLE_WAIT_TIME = 500;

    // Error codes
    private const int ERROR_DEBUGGER_ATTACH_TIMEOUT = 903;

    /// <summary>
    /// Entry point for CLI tool.
    /// </summary>
    static async Task<int> Main(string[] args)
    {
        var modulePath = Environment.CombineEnvironmentVariable(
            ModuleIntrinsics.GetPSModulePath(ModuleIntrinsics.PSModulePathScope.User),
            Path.Combine(Environment.GetRootedBasePath(AppContext.BaseDirectory), "Modules")
        );

        System.Environment.SetEnvironmentVariable("PSModulePath", modulePath, EnvironmentVariableTarget.Process);

        var execute = async (string[] args) =>
        {
            return await ClientBuilder.New().InvokeAsync(args);
        };

        if (AdapterBuilder.TryAdapter(args, out var adapterExecute))
        {
            execute = adapterExecute;
        }
        else if (ShouldWaitForDebugger(args))
        {
            if (!await WaitForDebuggerAsync())
                return ERROR_DEBUGGER_ATTACH_TIMEOUT;

            System.Diagnostics.Debugger.Break();
        }

        return await execute(args);
    }

    private static bool ShouldWaitForDebugger(string[] args)
    {
        if (args == null || args.Length == 0)
            return false;

        foreach (var arg in args)
        {
            if (arg.Equals("--wait-for-debugger", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Wait up to 5 minutes for the debugger to attach.
    /// </summary>
    private static async Task<bool> WaitForDebuggerAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Waiting for debugger to attach on PID {0}...", System.Diagnostics.Process.GetCurrentProcess().Id);

        try
        {
            var debuggerTimeoutToken = CancellationTokenSource.CreateLinkedTokenSource
            (
                cancellationToken,
                new CancellationTokenSource(TimeSpan.FromMinutes(DEBUGGER_ATTACH_TIMEOUT)).Token
            ).Token;

            while (!System.Diagnostics.Debugger.IsAttached)
            {
                await Task.Delay(DEBUGGER_ATTACH_CYCLE_WAIT_TIME, debuggerTimeoutToken);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}
