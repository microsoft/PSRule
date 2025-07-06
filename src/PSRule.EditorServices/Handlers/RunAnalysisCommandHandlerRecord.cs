// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// A record for the output of the run analysis command handler.
/// </summary>
public sealed class RunAnalysisCommandHandlerRecord
{
    /// <summary>
    /// Recommendation for the rule that failed or errored.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Name of the rule that failed or errored.
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Id of the rule that failed or errored.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;
}
