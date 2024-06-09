// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule;

/// <summary>
/// Extension for <see cref="ILogger"/> to log common messages.
/// </summary>
internal static class LoggerExtensions
{
    private static readonly EventId PSR0004 = new(4, "PSR0004");

    internal static void WarnResourceObsolete(this ILogger logger, ResourceKind kind, string id)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Warning))
            return;

        logger.LogWarning
        (
            PSRuleResources.ResourceObsolete,
            Enum.GetName(typeof(ResourceKind), kind),
            id
        );
    }

    internal static void ErrorResourceUnresolved(this ILogger logger, ResourceKind kind, string id)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0004,
            new PipelineBuilderException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0004,
                Enum.GetName(typeof(ResourceKind),
                kind), id
            )),
            PSRuleResources.PSR0004,
            Enum.GetName(typeof(ResourceKind), kind),
            id
        );
    }
}
