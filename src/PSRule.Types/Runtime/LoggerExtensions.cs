// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Extension for <see cref="ILogger"/> to log diagnostic messages.
/// </summary>
public static class LoggerExtensions
{
    private static readonly Func<FormattedLogValues, Exception?, string> _messageFormatter = MessageFormatter;

    /// <summary>
    /// Log an information level message.
    /// </summary>
    /// <param name="logger">A valid <see cref="ILogger"/> instance.</param>
    /// <param name="eventId">An event identifier for the warning.</param>
    /// <param name="message">The format message text.</param>
    /// <param name="args">Additional arguments to use within the format message.</param>
    public static void LogInformation(this ILogger logger, EventId eventId, string? message, params object?[] args)
    {
        logger.Log(LogLevel.Information, eventId, default, message, args);
    }

    /// <summary>
    /// Log an debug level message.
    /// </summary>
    /// <param name="logger">A valid <see cref="ILogger"/> instance.</param>
    /// <param name="eventId">An event identifier for the warning.</param>
    /// <param name="message">The format message text.</param>
    /// <param name="args">Additional arguments to use within the format message.</param>
    public static void LogDebug(this ILogger logger, EventId eventId, string? message, params object?[] args)
    {
        logger.Log(LogLevel.Debug, eventId, default, message, args);
    }

    /// <summary>
    /// Log a warning message.
    /// </summary>
    /// <param name="logger">A valid <see cref="ILogger"/> instance.</param>
    /// <param name="eventId">An event identifier for the warning.</param>
    /// <param name="message">The format message text.</param>
    /// <param name="args">Additional arguments to use within the format message.</param>
    public static void LogWarning(this ILogger logger, EventId eventId, string? message, params object?[] args)
    {
        logger.Log(LogLevel.Warning, eventId, default, message, args);
    }

    /// <summary>
    /// Log an error message.
    /// </summary>
    /// <param name="logger">A valid <see cref="ILogger"/> instance.</param>
    /// <param name="eventId">An event identifier for the error.</param>
    /// <param name="exception">An optional exception which the error message is related to.</param>
    /// <param name="message">The format message text.</param>
    /// <param name="args">Additional arguments to use within the format message.</param>
    public static void LogError(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.Log(LogLevel.Error, eventId, exception, message, args);
    }

    /// <summary>
    /// Log a diagnostic message.
    /// </summary>
    /// <param name="logger">A valid <see cref="ILogger"/> instance.</param>
    /// <param name="logLevel">The type of diagnostic message.</param>
    /// <param name="eventId">An event identifier for the diagnostic message.</param>
    /// <param name="exception">An optional exception which the diagnostic message is related to.</param>
    /// <param name="message">The format message text.</param>
    /// <param name="args">Additional arguments to use within the format message.</param>
    public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        if (logger == null || !logger.IsEnabled(logLevel))
            return;

        logger.Log(logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>
    /// Format log messages with values.
    /// </summary>
    private static string MessageFormatter(FormattedLogValues state, Exception? error)
    {
        return state.ToString();
    }
}
