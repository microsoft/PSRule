// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;
using System;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    internal static class IPipelineWriterExtensions
    {
        public static void DebugMessage(this IPipelineWriter logger, string message)
        {
            if (!logger.ShouldWriteDebug())
            {
                return;
            }

            logger.WriteDebug(new DebugRecord(message));
        }
    }

    internal static class PipelineWriterExtensions
    {
        internal static void WarnUsingInvariantCulture(this PipelineWriter writer)
        {
            if (!writer.ShouldWriteWarning())
                return;

            writer.WriteWarning(PSRuleResources.UsingInvariantCulture);
        }

        internal static void WarnRulePathNotFound(this PipelineWriter writer)
        {
            if (!writer.ShouldWriteWarning())
                return;

            writer.WriteWarning(PSRuleResources.RulePathNotFound);
        }

        internal static void WriteWarning(this PipelineWriter writer, string message, params object[] args)
        {
            if (!writer.ShouldWriteWarning() || string.IsNullOrEmpty(message))
                return;

            writer.WriteWarning(Format(message, args));
        }

        internal static void ErrorRequiredVersionMismatch(this PipelineWriter writer, string moduleName, string moduleVersion, string requiredVersion)
        {
            if (!writer.ShouldWriteError())
                return;

            writer.WriteError(
                new PipelineBuilderException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RequiredVersionMismatch, moduleName, moduleVersion, requiredVersion)),
                "PSRule.RequiredVersionMismatch",
                ErrorCategory.InvalidOperation
            );
        }

        internal static void ErrorReadFileFailed(this PipelineWriter writer, string path, Exception innerException)
        {
            if (!writer.ShouldWriteError())
                return;

            writer.WriteError(
                new PipelineSerializationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ReadFileFailed, path, innerException.Message), path, innerException),
                "PSRule.ReadFileFailed",
                ErrorCategory.InvalidData
            );
        }

        internal static void WriteError(this PipelineWriter writer, PipelineException exception, string errorId, ErrorCategory errorCategory)
        {
            writer.WriteError(new ErrorRecord(exception, errorId, errorCategory, null));
        }

        internal static void WriteDebug(this PipelineWriter writer, string message, params object[] args)
        {
            if (!writer.ShouldWriteDebug() || string.IsNullOrEmpty(message))
                return;

            writer.WriteDebug(new DebugRecord
            (
                message: Format(message, args)
            ));
        }

        private static string Format(string message, params object[] args)
        {
            return args == null || args.Length == 0 ? message : string.Format(Thread.CurrentThread.CurrentCulture, message, args);
        }
    }
}
