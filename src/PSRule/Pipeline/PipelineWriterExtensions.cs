// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
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

        internal static void WriteError(this PipelineWriter writer, PipelineException exception, string errorId, ErrorCategory errorCategory)
        {
            writer.WriteError(new ErrorRecord(exception, errorId, errorCategory, null));
        }
    }
}
