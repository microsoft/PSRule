// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;

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
    }
}
