// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a pipeline to return target objects.
/// </summary>
public interface IGetTargetPipelineBuilder : IPipelineBuilder
{
    /// <summary>
    /// Specifies a path for reading input objects from disk.
    /// </summary>
    void InputPath(string[] path);
}
