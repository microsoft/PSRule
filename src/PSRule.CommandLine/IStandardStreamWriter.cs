// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine;

/// <summary>
/// Represents a standard stream that can be written to.
/// </summary>
public interface IStandardStreamWriter
{
    /// <summary>
    /// Writes the specified string to the stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write(string? value);

    /// <summary>
    /// Writes the specified string followed by a line terminator to the stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteLine(string? value);
}
