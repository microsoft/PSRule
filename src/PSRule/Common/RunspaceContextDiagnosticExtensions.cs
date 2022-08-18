// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule
{
    internal static class RunspaceContextDiagnosticExtensions
    {
        private const string WARN_KEY_PROPERTY = "Property";

        internal static void WarnResourceObsolete(this RunspaceContext context, ResourceKind kind, string id)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning())
                return;

            context.Writer.WriteWarning(PSRuleResources.ResourceObsolete, Enum.GetName(typeof(ResourceKind), kind), id);
        }

        internal static void WarnPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
        {
            context.DebugPropertyObsolete(variableName, propertyName);
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.ShouldWarnOnce(WARN_KEY_PROPERTY, variableName, propertyName))
                return;

            context.Writer.WriteWarning(PSRuleResources.PropertyObsolete, variableName, propertyName);
        }

        internal static void WarnRuleNotFound(this RunspaceContext context)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning())
                return;

            context.Writer.WriteWarning(PSRuleResources.RuleNotFound);
        }

        internal static void WarnDuplicateRuleName(this RunspaceContext context, string ruleName)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning())
                return;

            context.Writer.WriteWarning(PSRuleResources.DuplicateRuleName, ruleName);
        }

        internal static void DuplicateResourceId(this RunspaceContext context, ResourceId id, ResourceId duplicateId)
        {
            if (context == null)
                return;

            var action = context.Pipeline.Option.Execution.DuplicateResourceId.GetValueOrDefault(ExecutionOption.Default.DuplicateResourceId.Value);
            if (action == ExecutionActionPreference.Error)
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DuplicateResourceId, id.Value, duplicateId.Value));

            else if (action == ExecutionActionPreference.Warn && context.Writer != null && context.Writer.ShouldWriteWarning())
                context.Writer.WriteWarning(PSRuleResources.DuplicateResourceId, id.Value, duplicateId.Value);
        }

        internal static void DebugPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteDebug())
                return;

            context.Writer.WriteDebug(PSRuleResources.DebugPropertyObsolete, context.RuleBlock.Name, variableName, propertyName);
        }

        internal static void WarnAliasReference(this RunspaceContext context, ResourceKind kind, string resourceId, string targetId, string alias)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.Pipeline.Option.Execution.AliasReferenceWarning.GetValueOrDefault(ExecutionOption.Default.AliasReferenceWarning.Value))
                return;

            context.Writer.WriteWarning(PSRuleResources.AliasReference, kind.ToString(), resourceId, targetId, alias);
        }

        internal static void WarnAliasSuppression(this RunspaceContext context, string targetId, string alias)
        {
            if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.Pipeline.Option.Execution.AliasReferenceWarning.GetValueOrDefault(ExecutionOption.Default.AliasReferenceWarning.Value))
                return;

            context.Writer.WriteWarning(PSRuleResources.AliasSuppression, targetId, alias);
        }
    }
}
