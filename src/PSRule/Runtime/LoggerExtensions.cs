// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Language;
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
    private static readonly EventId PSR0018 = new(18, "PSR0018");
    private static readonly EventId PSR0019 = new(19, "PSR0019");
    private static readonly EventId PSR0020 = new(20, "PSR0020");
    private static readonly EventId PSR0021 = new(21, "PSR0021");

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

    /// <summary>
    /// The resource name '{0}' is not valid at {1}. Each resource name must be between 3-128 characters in length, must start and end with a letter or number, and only contain letters, numbers, hyphens, dots, or underscores. See https://aka.ms/ps-rule/naming for more information.
    /// </summary>
    internal static void LogInvalidResourceName(this ILogger logger, string name, string extent)
    {
        logger.Log
        (
            LogLevel.Critical,
            PSR0018,
            new FormattedLogValues(PSRuleResources.InvalidResourceName, name, extent),
            new Pipeline.ParseException(
                eventId: PSR0018,
                message: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidResourceName, name, extent)
            ),
            (s, e) => s.ToString()
        );
    }

    /// <summary>
    /// Could not find required rule definition parameter '{0}' on rule at {1}.
    /// </summary>
    internal static void LogRuleParameterNotFound(this ILogger logger, string name, string extent)
    {
        logger.Log
        (
            LogLevel.Critical,
            PSR0019,
            new FormattedLogValues(PSRuleResources.RuleParameterNotFound, name, extent),
            new Pipeline.ParseException(
                eventId: PSR0019,
                message: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RuleParameterNotFound, name, extent)
            ),
            (s, e) => s.ToString()
        );
    }

    /// <summary>
    /// Rule nesting was detected for rule at {0}. Rules must not be nested.
    /// </summary>
    internal static void LogInvalidRuleNesting(this ILogger logger, string extent)
    {
        logger.Log
        (
            LogLevel.Critical,
            PSR0020,
            new FormattedLogValues(PSRuleResources.InvalidRuleNesting, extent),
            new Pipeline.ParseException(
                eventId: PSR0020,
                message: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidRuleNesting, extent)
            ),
            (s, e) => s.ToString()
        );
    }

    /// <summary>
    /// An invalid ErrorAction ({0}) was specified for rule at {1}. Ignore | Stop are supported.
    /// </summary>
    internal static void LogInvalidErrorAction(this ILogger logger, string action, string extent)
    {
        logger.Log
        (
            LogLevel.Critical,
            PSR0021,
            new FormattedLogValues(PSRuleResources.InvalidErrorAction, action, extent),
            new Pipeline.ParseException(
                eventId: PSR0021,
                message: string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidErrorAction, action, extent)
            ),
            (s, e) => s.ToString()
        );
    }

    internal static void VerboseRuleDiscovery(this ILogger logger, string path)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Trace) || string.IsNullOrEmpty(path))
            return;

        logger.LogVerbose(EventId.None, "[PSRule][D] -- Discovering rules in: {0}", path);
    }

    internal static void LogError(this ILogger logger, ErrorRecord errorRecord)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        var eventId = EventId.None;
        if (errorRecord.Exception is PipelineException pipelineException && pipelineException.EventId.HasValue)
        {
            eventId = pipelineException.EventId.Value;
        }

        logger.LogError(eventId, errorRecord.Exception, errorRecord.Exception.Message);
    }

    internal static void LogError(this ILogger logger, ParseError error)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Error))
            return;

        var exception = new Pipeline.ParseException(eventId: new EventId(0, error.ErrorId), message: error.Message);
        logger.LogError(new EventId(0, error.ErrorId), exception, exception.Message);
    }

    internal static void VerboseFoundResource(this ILogger logger, string name, string moduleName, string scriptName)
    {
        if (logger == null || !logger.IsEnabled(LogLevel.Trace))
            return;

        moduleName = string.IsNullOrEmpty(moduleName) ? "." : moduleName;
        logger.LogVerbose(EventId.None, "[PSRule][D] -- Found {0}\\{1} in {2}", moduleName, name, scriptName);
    }

    internal static void Throw(this ILogger logger, ExecutionActionPreference action, string message, params object[] args)
    {
        if (logger == null || action == ExecutionActionPreference.Ignore)
            return;

        if (action == ExecutionActionPreference.Error)
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, message, args));

        else if (action == ExecutionActionPreference.Warn && logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(EventId.None, message, args);

        else if (action == ExecutionActionPreference.Debug && logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(EventId.None, message, args);
    }
}
