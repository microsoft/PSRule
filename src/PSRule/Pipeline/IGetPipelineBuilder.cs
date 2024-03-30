// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a get pipeline.
/// </summary>
public interface IGetPipelineBuilder : IPipelineBuilder
{
    /// <summary>
    /// Determines if the returned rules also include rule dependencies.
    /// </summary>
    void IncludeDependencies();
}
