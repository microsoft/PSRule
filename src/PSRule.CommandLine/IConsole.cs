// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine;

/// <summary>
/// A console abstraction.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Write to the standard output stream.
    /// </summary>
    IStandardStreamWriter Out { get; }

    /// <summary>
    /// Write to the error stream.
    /// </summary>
    IStandardStreamWriter Error { get; }

    /// <summary>
    /// Open the standard input stream.
    /// </summary>
    Stream OpenStandardInput();

    /// <summary>
    /// Open the standard output stream.
    /// </summary>
    Stream OpenStandardOutput();
}
