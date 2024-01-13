// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Pipeline.Dependencies;
using PSRule.Tool.Models;

namespace PSRule.Tool.Commands;

/// <summary>
/// Execute features of the <c>run</c> command through the CLI.
/// </summary>
internal sealed class RunCommand
{
    private const string PUBLISHER = "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US";

    /// <summary>
    /// A generic error.
    /// </summary>
    private const int ERROR_GENERIC = 1;

    /// <summary>
    /// One or more failures occurred.
    /// </summary>
    private const int ERROR_BREAK_ON_FAILURE = 100;

    public static int Run(RunOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        var file = LockFile.Read(null);
        var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
            new string[] { Environment.GetWorkingPath() } : operationOptions.InputPath;

        if (operationOptions.Path != null)
            clientContext.Option.Include.Path = operationOptions.Path;

        if (operationOptions.Outcome != null && operationOptions.Outcome.Value != Rules.RuleOutcome.None)
            clientContext.Option.Output.Outcome = operationOptions.Outcome;

        // Build command
        var builder = CommandLineBuilder.Assert(operationOptions.Module ?? [], clientContext.Option, clientContext.Host, file);
        builder.Baseline(BaselineOption.FromString(operationOptions.Baseline));
        builder.InputPath(inputPath);
        builder.UnblockPublisher(PUBLISHER);

        using var pipeline = builder.Build();
        if (pipeline != null)
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
            if (pipeline.Result.HadFailures)
                exitCode = ERROR_BREAK_ON_FAILURE;
        }
        return clientContext.Host.HadErrors || pipeline == null ? ERROR_GENERIC : exitCode;
    }
}
