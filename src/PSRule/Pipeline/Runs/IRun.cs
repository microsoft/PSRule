// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Definitions;
using PSRule.Runtime;
using PSRule.Runtime.Binding;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A logical run.
/// Multiple runs can be created for a single pipeline execution.
/// </summary>
public interface IRun : IConfiguration
{
    /// <summary>
    /// An identifier for the run.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// A unique identifier for the run instance.
    /// </summary>
    string Guid { get; }

    /// <summary>
    /// A description of the logical run.
    /// </summary>
    InfoString? Description { get; }

    /// <summary>
    /// A correlation identifier for all related runs.
    /// </summary>
    string CorrelationGuid { get; }

    /// <summary>
    /// A list of rules that are part of the run.
    /// </summary>
    IRuleGraph Rules { get; }

    /// <summary>
    /// Evaluate bound properties for a target object.
    /// </summary>
    ITargetBindingResult Bind(ITargetObject targetObject);
}
