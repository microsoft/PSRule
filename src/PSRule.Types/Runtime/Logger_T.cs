// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Log diagnostic messages at runtime.
/// </summary>
/// <typeparam name="T">The type name to use for the logger category.</typeparam>
public sealed class Logger<T>(ILoggerFactory loggerFactory) : ILogger<T>
{
    private readonly ILogger _Logger = loggerFactory.Create(typeof(T).FullName);

    /// <summary>
    /// The name of the category.
    /// </summary>
    public string CategoryName => typeof(T).Name;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _Logger.IsEnabled(logLevel);
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _Logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
