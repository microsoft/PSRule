// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Language;
using PSRule.Resources;

namespace PSRule.Pipeline;

/// <summary>
/// Extensions for the <see cref="IPipelineWriter"/>.
/// </summary>
public static class PipelineWriterExtensions
{
    /// <summary>
    /// Write a debug message.
    /// </summary>
    public static void WriteDebug(this IPipelineWriter writer, DebugRecord debugRecord)
    {
        if (debugRecord == null)
            return;

        writer.WriteDebug(debugRecord.Message);
    }

    internal static void DebugMessage(this IPipelineWriter logger, string message)
    {
        if (logger == null || !logger.ShouldWriteDebug())
            return;

        logger.WriteDebug(new DebugRecord(message));
    }

    internal static void WriteWarning(this IPipelineWriter writer, string message, params object[] args)
    {
        if (writer == null || !writer.ShouldWriteWarning() || string.IsNullOrEmpty(message))
            return;

        writer.WriteWarning(Format(message, args));
    }

    internal static void ErrorRequiredVersionMismatch(this IPipelineWriter writer, string moduleName, string moduleVersion, string requiredVersion)
    {
        if (writer == null || !writer.ShouldWriteError())
            return;

        writer.WriteError(
            new PipelineBuilderException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RequiredVersionMismatch, moduleName, moduleVersion, requiredVersion)),
            "PSRule.RequiredVersionMismatch",
            ErrorCategory.InvalidOperation
        );
    }

    internal static void ErrorReadFileFailed(this IPipelineWriter writer, string path, Exception innerException)
    {
        if (writer == null || !writer.ShouldWriteError())
            return;

        writer.WriteError(
            new PipelineSerializationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ReadFileFailed, path, innerException.Message), path, innerException),
            "PSRule.ReadFileFailed",
            ErrorCategory.InvalidData
        );
    }

    internal static void ErrorReadInputFailed(this IPipelineWriter writer, string path, Exception innerException)
    {
        if (writer == null || !writer.ShouldWriteError())
            return;

        writer.WriteError(
            new PipelineSerializationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ReadInputFailed, path, innerException.Message), path, innerException),
            "PSRule.ReadInputFailed",
            ErrorCategory.ReadError
        );
    }

    internal static void WriteError(this IPipelineWriter writer, PipelineException exception, string errorId, ErrorCategory errorCategory)
    {
        if (writer == null)
            return;

        writer.WriteError(new ErrorRecord(exception, errorId, errorCategory, null));
    }

    internal static void WriteError(this IPipelineWriter writer, ParseError error)
    {
        if (writer == null || !writer.ShouldWriteError())
            return;

        var record = new ErrorRecord
        (
            exception: new Pipeline.ParseException(message: error.Message, errorId: error.ErrorId),
            errorId: error.ErrorId,
            errorCategory: ErrorCategory.InvalidOperation,
            targetObject: null
        );
        writer.WriteError(errorRecord: record);
    }

    internal static void WriteDebug(this IPipelineWriter writer, string message, params object[] args)
    {
        if (writer == null || !writer.ShouldWriteDebug() || string.IsNullOrEmpty(message))
            return;

        writer.WriteDebug(new DebugRecord
        (
            message: Format(message, args)
        ));
    }

    internal static void VerboseRuleDiscovery(this IPipelineWriter writer, string path)
    {
        if (writer == null || !writer.ShouldWriteVerbose() || string.IsNullOrEmpty(path))
            return;

        writer.WriteVerbose($"[PSRule][D] -- Discovering rules in: {path}");
    }

    private static string Format(string message, params object[] args)
    {
        return args == null || args.Length == 0 ? message : string.Format(Thread.CurrentThread.CurrentCulture, message, args);
    }
}
