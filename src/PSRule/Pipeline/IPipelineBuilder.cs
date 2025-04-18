// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A helper to build a PSRule pipeline.
/// </summary>
public interface IPipelineBuilder
{
    /// <summary>
    /// Configure the pipeline with options.
    /// </summary>
    IPipelineBuilder Configure(PSRuleOption option);

    /// <summary>
    /// Configure the pipeline to use a specific baseline.
    /// </summary>
    /// <param name="baseline">A baseline option or the name of a baseline.</param>
    void Baseline(BaselineOption? baseline);

    /// <summary>
    /// Build the pipeline.
    /// </summary>
    /// <param name="writer">Optionally specify a custom writer which will handle output processing.</param>
    IPipeline? Build(IPipelineWriter? writer = null);
}

#nullable restore
