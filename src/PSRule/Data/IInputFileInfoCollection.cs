// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A collection of <see cref="InputFileInfo"/>.
/// </summary>
public interface IInputFileInfoCollection : IEnumerable<InputFileInfo>
{
    /// <summary>
    /// Filters the collection to only include <see cref="InputFileInfo"/> with a specific file extension.
    /// </summary>
    /// <param name="extension">A file extension to filter the collection to.</param>
    /// <returns>A filtered collection.</returns>
    IInputFileInfoCollection WithExtension(string extension);
}
