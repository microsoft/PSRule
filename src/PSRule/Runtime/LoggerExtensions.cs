// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Options;
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
    private static readonly EventId PSR0008 = new(8, "PSR0008");
    private static readonly EventId PSR0009 = new(9, "PSR0009");
    private static readonly EventId PSR0010 = new(10, "PSR0010");
    private static readonly EventId PSR0011 = new(11, "PSR0011");
    private static readonly EventId PSR0015 = new(15, "PSR0015");
    private static readonly EventId PSR0016 = new(16, "PSR0016");
    private static readonly EventId PSR0017 = new(17, "PSR0017");


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

    /// <summary>
    /// PSR0008: The capability '{0}' requested by the workspace is disabled.
    /// </summary>
    internal static void ErrorWorkspaceCapabilityDisabled(this ILogger logger, string capability)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0008,
            new PipelineCapabilityException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0008,
                capability
            ), capability),
            PSRuleResources.PSR0008,
            capability
        );
    }

    /// <summary>
    /// PSR0009: The capability '{0}' requested by the workspace is not supported.
    /// </summary>
    internal static void ErrorWorkspaceCapabilityNotSupported(this ILogger logger, string capability)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0009,
            new PipelineCapabilityException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0009,
                capability
            ), capability),
            PSRuleResources.PSR0009,
            capability
        );
    }

    /// <summary>
    /// PSR0010: The capability '{0}' requested by the module '{1}' is disabled.
    /// </summary>
    internal static void ErrorModuleCapabilityDisabled(this ILogger logger, string capability, string module)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0010,
            new PipelineCapabilityException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0010,
                capability,
                module
            ), capability, module),
            PSRuleResources.PSR0010,
            capability,
            module
        );
    }

    /// <summary>
    /// PSR0011: The capability '{0}' requested by the module '{1}' is not supported.
    /// </summary>
    internal static void ErrorModuleCapabilityNotSupported(this ILogger logger, string capability, string module)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        logger.LogError
        (
            PSR0011,
            new PipelineCapabilityException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0011,
                capability,
                module
            ), capability, module),
            PSRuleResources.PSR0011,
            capability,
            module
        );
    }

    /// <summary>
    /// PSR0015: No valid sources were found. Please check your working path and configured options.
    /// </summary>
    internal static void LogNoValidSources(this ILogger logger, ExecutionActionPreference actionPreference)
    {
        logger.Log
        (
            actionPreference,
            PSR0015,
            new PipelineBuilderException(PSR0015, PSRuleResources.PSR0015),
            PSRuleResources.PSR0015
        );
    }

    /// <summary>
    /// PSR0016: Could not find a matching rule. Please check that Path, Name, and Tag parameters are correct.
    /// </summary>
    internal static void LogNoMatchingRules(this ILogger logger, ExecutionActionPreference actionPreference)
    {
        logger.Log
        (
            actionPreference,
            PSR0016,
            new PipelineBuilderException(PSR0016, PSRuleResources.PSR0016),
            PSRuleResources.PSR0016
        );
    }

    /// <summary>
    /// PSR0017: No valid input objects or files were found. Please check your working path and configured options.
    /// </summary>
    internal static void LogNoValidInput(this ILogger logger, ExecutionActionPreference actionPreference)
    {
        logger.Log
        (
            actionPreference,
            PSR0017,
            new PipelineBuilderException(PSR0017, PSRuleResources.PSR0017),
            PSRuleResources.PSR0017
        );
    }
}
