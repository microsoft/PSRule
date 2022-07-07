// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule
{
    public static class PipelineWriterExtensions
    {
        public static void WriteDebug(this IPipelineWriter writer, DebugRecord debugRecord)
        {
            if (debugRecord == null)
                return;

            writer.WriteDebug(debugRecord.Message);
        }
    }
}
