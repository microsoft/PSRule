// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A logger that sinks all logs.
/// </summary>
public sealed class NullLogger<T> : ILogger<T>
{
    /// <summary>
    /// An default instance of the null logger.
    /// </summary>
    public static readonly NullLogger<T> Instance = new();

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        
    }
}
