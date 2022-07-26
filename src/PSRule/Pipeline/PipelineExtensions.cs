// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// Extension methods for the PSRule pipelines.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Process an object through the pipeline. Each object will be processed by rules that apply based on pre-conditions.
        /// </summary>
        /// <param name="pipeline">An instance of a PSRule pipeline.</param>
        /// <param name="sourceObject">The object to process.</param>
        public static void Process(this IPipeline pipeline, object sourceObject)
        {
            if (pipeline == null)
                return;

            pipeline.Process(PSObject.AsPSObject(sourceObject));
        }
    }
}
