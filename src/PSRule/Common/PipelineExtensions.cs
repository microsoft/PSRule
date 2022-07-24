// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule
{
    public static class PipelineExtensions
    {
        /// <summary>
        /// Process an object through the pipeline. Each object will be processed by rules that apply based on pre-conditions.
        /// </summary>
        /// <param name="sourceObject">The object to process.</param>
        public static void Process(this IPipeline pipeline, object sourceObject)
        {
            pipeline.Process(PSObject.AsPSObject(sourceObject));
        }
    }
}
