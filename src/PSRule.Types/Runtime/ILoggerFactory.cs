// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A factory that creates loggers.
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// A factory for creating loggers.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>Create an instance of an <see cref="ILogger"/> with the specified category name.</returns>
    ILogger Create(string categoryName);
}
