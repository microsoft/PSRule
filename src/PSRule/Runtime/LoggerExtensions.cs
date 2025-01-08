// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Runtime;

/// <summary>
/// Extension for <see cref="ILogger"/> to log common messages.
/// </summary>
internal static class LoggerExtensions
{
    private static readonly EventId PSR0004 = new(4, "PSR0004");
    private static readonly EventId PSR0005 = new(5, "PSR0005");
    private static readonly EventId PSR0006 = new(6, "PSR0006");
    private static readonly EventId PSR0007 = new(7, "PSR0007");

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

    /// <summary>
    /// PSR0006: Failed to deserialize the file '{0}': {1}
    /// </summary>
    internal static void ErrorReadFileFailed(this ILogger logger, string path, Exception innerException)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0006,
            new PipelineSerializationException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0006,
                path,
                innerException.Message), path, innerException
            ),
            PSRuleResources.PSR0006,
            path,
            innerException.Message
        );
    }

    /// <summary>
    /// PSR0007: The resource '{0}' using API '{1}' is not recognized as a valid PSRule resource (source: {2}).
    /// </summary>
    internal static void LogUnknownResourceKind(this ILogger logger, string kind, string apiVersion, ISourceFile file)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogWarning
        (
            PSR0007,
            PSRuleResources.PSR0007,
            kind,
            apiVersion,
            file.Path
        );
    }
}
