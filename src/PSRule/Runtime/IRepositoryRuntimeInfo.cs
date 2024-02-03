// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Runtime;

/// <summary>
/// Display information about the current repository at runtime.
/// </summary>
public interface IRepositoryRuntimeInfo
{
    /// <summary>
    /// A URL to the current repository.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// The base ref for the current repository branch.
    /// </summary>
    string BaseRef { get; }

    /// <summary>
    /// Get a list of changed files within the repository.
    /// </summary>
    /// <returns>A collection of files.</returns>
    IInputFileInfoCollection GetChangedFiles();
}
