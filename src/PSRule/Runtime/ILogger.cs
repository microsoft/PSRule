// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A generic interface for diagnostic logging within PSRule.
/// </summary>
internal interface ILogger
{
    /// <summary>
    /// Determines if a specific log level should be written.
    /// </summary>
    /// <param name="level">The level to query.</param>
    /// <returns>Returns <c>true</c> when the log level should be written or <c>false</c> otherwise.</returns>
    bool ShouldLog(LogLevel level);

    /// <summary>
    /// Write a warning.
    /// </summary>
    /// <param name="message">The warning message write.</param>
    /// <param name="args">Any arguments to format the string with.</param>
    void Warning(string message, params object[] args);

    /// <summary>
    /// Write an error from an exception.
    /// </summary>
    /// <param name="exception">The exception to write.</param>
    /// <param name="errorId">A string identifier for the error.</param>
    void Error(Exception exception, string errorId = null);
}
