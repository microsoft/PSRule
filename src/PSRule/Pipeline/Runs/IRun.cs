// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Pipeline.Runs;

#nullable enable

/// <summary>
/// A logical run.
/// Multiple runs can be created for a single pipeline execution.
/// </summary>
public interface IRun : IConfiguration
{
    /// <summary>
    /// A unique identifier for the run.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// A description of the logical run.
    /// </summary>
    InfoString? Description { get; }

    /// <summary>
    /// A correlation identifier for all related runs.
    /// </summary>
    string CorrelationGuid { get; }
}

#nullable restore
