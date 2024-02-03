// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A collection of input passed to PSRule for anlaysis.
/// </summary>
public interface IInputCollection
{
    /// <summary>
    /// Add a path to the list of inputs.
    /// </summary>
    /// <param name="path">The path of files to add.</param>
    void Add(string path);
}
