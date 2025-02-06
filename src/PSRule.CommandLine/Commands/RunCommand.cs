// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.CommandLine.Models;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Pipeline.Dependencies;

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Execute features of the <c>run</c> command through the CLI.
/// </summary>
public sealed class RunCommand
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

    /// <summary>
    /// Call <c>run</c>.
    /// </summary>
    public static async Task<int> RunAsync(RunOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        var exitCode = 0;
        var file = LockFile.Read(null);
        var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
            [Environment.GetWorkingPath()] : operationOptions.InputPath;

        if (operationOptions.Path != null)
        {
            clientContext.Option.Include.Path = operationOptions.Path;
        }

        if (operationOptions.Outcome != null && operationOptions.Outcome.Value != Rules.RuleOutcome.None)
        {
            clientContext.Option.Output.Outcome = operationOptions.Outcome;
        }

        if (operationOptions.OutputPath != null && operationOptions.OutputFormat != null && operationOptions.OutputFormat.Value != OutputFormat.None)
        {
            clientContext.Option.Output.Path = operationOptions.OutputPath;
            clientContext.Option.Output.Format = operationOptions.OutputFormat.Value;
        }

        // Run restore command.
        if (!operationOptions.NoRestore)
        {
            exitCode = await ModuleCommand.ModuleRestoreAsync(new RestoreOptions
            {
                Path = operationOptions.Path,
                WriteOutput = false,
            }, clientContext, cancellationToken);
        }

        // Build command.
        var builder = CommandLineBuilder.Assert(operationOptions.Module ?? [], clientContext.Option, clientContext.Host, file, clientContext.ResolvedModuleVersions);
        builder.Baseline(Configuration.BaselineOption.FromString(operationOptions.Baseline));
        builder.InputPath(inputPath);
        builder.UnblockPublisher(PUBLISHER);
        builder.Formats(operationOptions.Formats);

        using var pipeline = builder.Build();
        if (pipeline != null)
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
            if (pipeline.Result.ShouldBreakFromFailure)
                exitCode = ERROR_BREAK_ON_FAILURE;
        }
        exitCode = clientContext.Host.HadErrors || pipeline == null ? ERROR_GENERIC : exitCode;
        clientContext.LogVerbose("[PSRule][R] -- Completed run with exit code {0}.", exitCode);
        return await Task.FromResult(exitCode);
    }
}
