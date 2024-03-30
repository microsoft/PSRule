// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a pipeline for getting help from rules.
/// </summary>
public interface IHelpPipelineBuilder : IPipelineBuilder
{
    /// <summary>
    /// Get the full help output for a rule.
    /// </summary>
    void Full();

    /// <summary>
    /// Open or show online help for a rule if it exists.
    /// </summary>
    void Online();

    /// <summary>
    /// Filter by name.
    /// </summary>
    void Name(string[] name);
}
