// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.Pipeline;
using PSRule.Rules;
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
            var problems = GetOnlyProblemRecords(output.Results);

            return new RunAnalysisCommandHandlerOutput(output.ExitCode, problems);
        }
        catch (Exception ex)
        {
            _Logger.LogError(EventId.None, ex, ex.Message);
        }

        return new RunAnalysisCommandHandlerOutput(1);
    }

    private static IEnumerable<RunAnalysisCommandHandlerRecord> GetOnlyProblemRecords(IReadOnlyCollection<InvokeResult> results)
    {
        foreach (var result in results)
        {
            if (result.IsSuccess())
                continue;

            foreach (var record in result)
            {
                if (record.Outcome != RuleOutcome.Fail && record.Outcome != RuleOutcome.Error)
                    continue;

                if (record.Recommendation != null)
                {
                    yield return new RunAnalysisCommandHandlerRecord
                    {
                        Recommendation = record.Recommendation,
                        RuleName = record.RuleName,
                        RuleId = record.RuleId
                    };
                }
            }
        }
    }
}
