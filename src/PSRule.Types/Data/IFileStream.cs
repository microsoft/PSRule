// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A stream associated with a file on disk.
/// </summary>
public interface IFileStream : IDisposable
{
    /// <summary>
    /// Get the file stream as a text reader.
    /// </summary>
    /// <returns></returns>
    TextReader AsTextReader();

    /// <summary>
    /// Information about the file.
    /// </summary>
    IFileInfo Info { get; }
}
