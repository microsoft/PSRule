// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// An object with information about an input file.
/// </summary>
public interface IFileInfo
{
    /// <summary>
    /// The full path to the file.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// The extension for the file.
    /// </summary>
    string? Extension { get; }

    /// <summary>
    /// Get a stream for the file.
    /// </summary>
    IFileStream GetFileStream();
}
