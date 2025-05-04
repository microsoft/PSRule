// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Log diagnostic messages at runtime.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Determine if the specified type of diagnostic message should be logged.
    /// </summary>
    /// <param name="logLevel">The type of the diagnostic message.</param>
    /// <returns>Returns <c>true</c> if the <see cref="LogLevel"/> should be logged.</returns>
    public bool IsEnabled(LogLevel logLevel);

    /// <summary>
    /// Log a diagnostic message.
    /// </summary>
    /// <typeparam name="TState">Additional information that describes the diagnostic state to log.</typeparam>
    /// <param name="logLevel">The type of the diagnostic message.</param>
    /// <param name="eventId">An event identifier for the diagnostic message.</param>
    /// <param name="state">Additional information that describes the diagnostic state to log.</param>
    /// <param name="exception">An optional exception which the diagnostic message is related to.</param>
    /// <param name="formatter">A function to format the diagnostic message for the output stream.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
}
