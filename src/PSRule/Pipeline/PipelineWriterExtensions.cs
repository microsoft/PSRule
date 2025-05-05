// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Language;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// Extensions for the <see cref="IPipelineWriter"/>.
/// </summary>
public static class PipelineWriterExtensions
{
    internal static void WriteWarning(this IPipelineWriter writer, string message, params object[] args)
    {
        writer?.Log(LogLevel.Warning, EventId.None, message, null, (s, e) => Format(message, args));
    }

    internal static void ErrorRequiredVersionMismatch(this IPipelineWriter writer, string moduleName, string moduleVersion, string requiredVersion)
    {
        writer?.WriteError(
            new PipelineBuilderException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RequiredVersionMismatch, moduleName, moduleVersion, requiredVersion)),
            "PSRule.RequiredVersionMismatch",
            ErrorCategory.InvalidOperation
        );
    }

    internal static void ErrorReadFileFailed(this IPipelineWriter writer, string path, Exception innerException)
    {
        writer?.WriteError(
            new PipelineSerializationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ReadFileFailed, path, innerException.Message), path, innerException),
            "PSRule.ReadFileFailed",
            ErrorCategory.InvalidData
        );
    }

    internal static void ErrorReadInputFailed(this IPipelineWriter writer, string path, Exception innerException)
    {
        writer?.WriteError(
            new PipelineSerializationException(new EventId(0, "PSRule.ReadInputFailed"), string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ReadInputFailed, path, innerException.Message), path, innerException),
            "PSRule.ReadInputFailed",
            ErrorCategory.ReadError
        );
    }

    internal static void WriteError(this IPipelineWriter writer, PipelineException exception, string errorId, ErrorCategory errorCategory)
    {
        writer?.LogError(new ErrorRecord(exception, errorId, errorCategory, null));
    }

    internal static void WriteError(this IPipelineWriter writer, ParseError error)
    {
        var record = new ErrorRecord
        (
            // TODO: Fix event id 0
            exception: new ParseException(eventId: new EventId(0, error.ErrorId), message: error.Message),
            errorId: error.ErrorId,
            errorCategory: ErrorCategory.InvalidOperation,
            targetObject: null
        );
        writer?.LogError(errorRecord: record);
    }

    internal static void WriteDebug(this IPipelineWriter writer, string message, params object[] args)
    {
        writer?.Log(LogLevel.Debug, EventId.None, message, null, (s, e) => Format(message, args));
    }

    private static string Format(string message, params object[] args)
    {
        return args == null || args.Length == 0 ? message : string.Format(Thread.CurrentThread.CurrentCulture, message, args);
    }
}
