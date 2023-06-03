// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule
{
    internal static class LoggerExtensions
    {
        internal static void WarnResourceObsolete(this ILogger logger, ResourceKind kind, string id)
        {
            if (logger == null || !logger.ShouldLog(LogLevel.Warning))
                return;

            logger.Warning(PSRuleResources.ResourceObsolete, Enum.GetName(typeof(ResourceKind), kind), id);
        }

        internal static void ErrorResourceUnresolved(this ILogger logger, ResourceKind kind, string id)
        {
            if (logger == null || !logger.ShouldLog(LogLevel.Error))
                return;

            logger.Error(new PipelineBuilderException(string.Format(
                Thread.CurrentThread.CurrentCulture,
                PSRuleResources.PSR0004,
                Enum.GetName(typeof(ResourceKind),
                kind), id
            )), "PSR0004");
        }
    }
}
