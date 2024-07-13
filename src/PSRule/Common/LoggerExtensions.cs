// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// Extension for <see cref="ILogger"/> to log common messages.
/// </summary>
internal static class LoggerExtensions
{
    private static readonly EventId PSR0004 = new(4, "PSR0004");
    private static readonly EventId PSR0005 = new(5, "PSR0005");

    /// <summary>
    /// PSR0005: The {0} '{1}' is obsolete.
    /// </summary>
    internal static void WarnResourceObsolete(this ILogger logger, ResourceKind kind, string id)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Warning))
            return;

        logger.LogWarning
        (
            PSR0005,
            PSRuleResources.PSR0005,
            Enum.GetName(typeof(ResourceKind), kind),
            id
        );
    }

    /// <summary>
    /// PSR0004: The specified {0} resource '{1}' is not known.
    /// </summary>
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
