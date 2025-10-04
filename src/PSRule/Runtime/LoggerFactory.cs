// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Implements creating a logger for a specific category.
/// </summary>
internal sealed class LoggerFactory : ILoggerFactory
{
    public ILogger? Create(string categoryName)
    {
        return LegacyRunspaceContext.CurrentThread == null ? LegacyRunspaceContext.CurrentThread : NullLogger.Instance;
    }
}
