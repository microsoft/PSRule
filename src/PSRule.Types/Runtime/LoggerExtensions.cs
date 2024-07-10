// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// 
/// </summary>
public static class LoggerExtensions
{
    public static void LogWarning(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
       if (logger == null || !logger.IsEnabled(LogLevel.Warning))
            return;

        logger.Log(LogLevel.Warning, eventId, exception, message, args);
    }
}
