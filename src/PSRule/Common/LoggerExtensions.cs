// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions;
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
    }
}
