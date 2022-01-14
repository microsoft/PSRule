// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule
{
    internal static class RunspaceContextExtensions
    {
        private const string WARN_KEY_PROPERTY = "Property";

        public static void WarnResourceObsolete(this RunspaceContext context, ResourceKind kind, string id)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning())
                return;

            context.Writer.WriteWarning(PSRuleResources.ResourceObsolete, Enum.GetName(typeof(ResourceKind), kind), id);
        }

        public static void WarnPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
        {
            context.DebugPropertyObsolete(variableName, propertyName);
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.ShouldWarnOnce(WARN_KEY_PROPERTY, variableName, propertyName))
                return;

            context.Writer.WriteWarning(PSRuleResources.PropertyObsolete, variableName, propertyName);
        }

        public static void WarnRuleNotFound(this RunspaceContext context)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning())
                return;

            context.Writer.WriteWarning(PSRuleResources.RuleNotFound);
        }

        public static void DebugPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteDebug())
                return;

            context.Writer.WriteDebug(PSRuleResources.DebugPropertyObsolete, context.RuleBlock.Name, variableName, propertyName);
        }

        public static void WarnAliasReference(this RunspaceContext context, ResourceKind kind, string resourceId, string targetId, string alias)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.Pipeline.Option.Execution.AliasReferenceWarning.GetValueOrDefault(ExecutionOption.Default.AliasReferenceWarning.Value))
                return;

            context.Writer.WriteWarning(PSRuleResources.AliasReference, kind.ToString(), resourceId, targetId, alias);
        }

        public static void WarnAliasSuppression(this RunspaceContext context, string targetId, string alias)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.Pipeline.Option.Execution.AliasReferenceWarning.GetValueOrDefault(ExecutionOption.Default.AliasReferenceWarning.Value))
                return;

            context.Writer.WriteWarning(PSRuleResources.AliasSuppression, targetId, alias);
        }
    }
}
