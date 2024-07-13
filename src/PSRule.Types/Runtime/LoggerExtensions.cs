// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// 
/// </summary>
public static class LoggerExtensions
{
    private static readonly Func<FormattedLogValues, Exception?, string> _messageFormatter = MessageFormatter;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="eventId"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogWarning(this ILogger logger, EventId eventId, string? message, params object?[] args)
    {
       if (logger == null || !logger.IsEnabled(LogLevel.Warning))
            return;

        logger.Log(LogLevel.Warning, eventId, default, message, args);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogError(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.Log(LogLevel.Error, eventId, exception, message, args);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <exception cref="NullReferenceException"></exception>
    public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        if (logger == null) throw new NullReferenceException(nameof(logger));

        logger.Log(logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    private static string MessageFormatter(FormattedLogValues state, Exception? error)
    {
        return state.ToString();
    }
}
