// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A result from the pipeline.
/// </summary>
public interface IPipelineResult
{
    /// <summary>
    /// Determines if any errors were reported.
    /// </summary>
    public bool HadErrors { get; }

    /// <summary>
    /// Determines if an failures were reported.
    /// </summary>
    public bool HadFailures { get; }

    /// <summary>
    /// Determines if the pipeline should break from rules that failed.
    /// </summary>
    public bool ShouldBreakFromFailure { get; }
}
