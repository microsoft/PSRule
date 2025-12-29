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
    public static async Task<RunCommandOutput> RunAsync(RunOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        var exitCode = 0;
        var workingPath = operationOptions.WorkspacePath ?? Environment.GetWorkingPath();
        var file = LockFile.Read(null);
        var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
            [workingPath] : operationOptions.InputPath;

        if (operationOptions.Path != null)
        {
            clientContext.Option.Include.Path = operationOptions.Path;
        }

        if (operationOptions.Convention != null && operationOptions.Convention.Length > 0)
        {
            clientContext.Option.Convention.Include = operationOptions.Convention;
        }

        if (operationOptions.Outcome != null && operationOptions.Outcome.Value != Rules.RuleOutcome.None)
        {
            clientContext.Option.Output.Outcome = operationOptions.Outcome;
        }

        if (operationOptions.BreakLevel != null)
        {
            clientContext.Option.Execution.Break = operationOptions.BreakLevel.Value;
        }

        if (operationOptions.OutputPath != null && operationOptions.OutputFormat != null && operationOptions.OutputFormat.Value != OutputFormat.None)
        {
            clientContext.Option.Output.Path = operationOptions.OutputPath;
            clientContext.Option.Output.Format = operationOptions.OutputFormat.Value;
        }

        if (operationOptions.JobSummaryPath != null)
        {
            clientContext.Option.Output.JobSummaryPath = operationOptions.JobSummaryPath;
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
        var results = new List<InvokeResult>();
        var sessionContext = new SessionContext(clientContext.Host)
        {
            WorkingPath = workingPath,
            OnWriteResult = results.Add
        };
        var builder = CommandLineBuilder.Assert(operationOptions.Module ?? [], clientContext.Option, sessionContext, file, clientContext.ResolvedModuleVersions);
        builder.Name(operationOptions.Name);
        builder.Baseline(BaselineOption.FromString(operationOptions.Baseline));
        builder.InputPath(inputPath);
        builder.UnblockPublisher(PUBLISHER);
        builder.Formats(operationOptions.Formats);

        using var pipeline = builder.Build();
        if (pipeline != null && clientContext.Host.ExitCode == 0)
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
            if (pipeline.Result.ShouldBreakFromFailure)
                exitCode = ERROR_BREAK_ON_FAILURE;
        }

        exitCode = clientContext.Host.HadErrors || pipeline == null ? ERROR_GENERIC : exitCode;
        if (clientContext.LastErrorCode.HasValue && clientContext.LastErrorCode.Value != 0)
        {
            exitCode = clientContext.LastErrorCode.Value;
        }

        clientContext.LogVerbose("[PSRule][R] -- Completed run with exit code {0}.", exitCode);
        return await Task.FromResult(new RunCommandOutput(exitCode, results));
    }
}
