// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.Runtime;

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Handler for running analysis.
/// </summary>
public sealed class RunAnalysisCommandHandler(ClientContext context, ISerializer serializer, ILogger logger)
    : ExecuteTypedResponseCommandHandlerBase<RunAnalysisCommandHandlerInput, RunAnalysisCommandHandlerOutput>(COMMAND_NAME, serializer)
{
    private const string COMMAND_NAME = "runAnalysis";

    private readonly ClientContext _Context = context;
    private readonly ILogger _Logger = logger;

    /// <summary>
    /// Handle the <c>runAnalysis</c> command.
    /// </summary>
    public override async Task<RunAnalysisCommandHandlerOutput> Handle(RunAnalysisCommandHandlerInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        ArgumentException.ThrowIfNullOrWhiteSpace(input.WorkspacePath, nameof(input.WorkspacePath));

        var options = new RunOptions
        {
            InputPath = input.InputPath,
            WorkspacePath = input.WorkspacePath,
            BreakLevel = Options.BreakLevel.Never,
        };

        _Logger.LogInformation(EventId.None, "Running tests in: {0}", input.WorkspacePath);
        _Logger.LogInformation(EventId.None, "Input path: {0}", string.Join(", ", input.InputPath ?? []));

        try
        {
            var output = await RunCommand.RunAsync(options, _Context, cancellationToken);
            return new RunAnalysisCommandHandlerOutput(output.ExitCode);
        }
        catch (Exception ex)
        {
            _Logger.LogError(EventId.None, ex, ex.Message);
        }

        return new RunAnalysisCommandHandlerOutput(1);
    }
}
