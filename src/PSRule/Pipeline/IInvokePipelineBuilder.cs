// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A helper to build a pipeline for executing rules and conventions within a PSRule sandbox.
/// </summary>
public interface IInvokePipelineBuilder : IPipelineBuilder
{
    /// <summary>
    /// Configures the name of the rules to run.
    /// </summary>
    void Name(string[]? name);

    /// <summary>
    /// Configures paths that will be scanned for input.
    /// </summary>
    /// <param name="path">An array of relative or absolute path specs to be scanned. Directories will be recursively scanned for all files not excluded matching the file path spec.</param>
    void InputPath(string[]? path);

    /// <summary>
    /// Configures a variable that will receive all results in addition to the host context.
    /// </summary>
    /// <param name="variableName">The name of the variable to set.</param>
    void ResultVariable(string variableName);

    /// <summary>
    /// Unblocks PowerShell sources from trusted publishers that originate from an Internet zone.
    /// </summary>
    /// <param name="publisher">The trusted publisher to unblock.</param>
    void UnblockPublisher(string publisher);

    /// <summary>
    /// Enables one or more formats to be used when reading input objects.
    /// </summary>
    void Formats(string[]? format);
}

#nullable restore
