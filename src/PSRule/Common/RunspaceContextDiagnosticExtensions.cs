// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

internal static class RunspaceContextDiagnosticExtensions
{
    private const string WARN_KEY_PROPERTY = "Property";

    internal static void WarnPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
    {
        context.DebugPropertyObsolete(variableName, propertyName);
        if (context.Writer == null || !context.Writer.ShouldWriteWarning() || !context.ShouldWarnOnce(WARN_KEY_PROPERTY, variableName, propertyName))
            return;

        context.Writer.WriteWarning(PSRuleResources.PropertyObsolete, variableName, propertyName);
    }

    /// <summary>
    /// The option '{0}' is deprecated and will be removed with PSRule v3. See http://aka.ms/ps-rule/deprecations for more detail.
    /// </summary>
    internal static void WarnDeprecatedOption(this RunspaceContext context, string option)
    {
        if (context.Writer == null || !context.Writer.ShouldWriteWarning())
            return;

        context.Writer.WriteWarning(PSRuleResources.DeprecatedOption, option);
    }

    internal static void WarnDuplicateRuleName(this RunspaceContext context, string ruleName)
    {
        if (context == null || context.Writer == null || !context.Writer.ShouldWriteWarning())
            return;

        context.Writer.WriteWarning(PSRuleResources.DuplicateRuleName, ruleName);
    }

    internal static void DuplicateResourceId(this RunspaceContext context, ResourceId id, ResourceId duplicateId)
    {
        if (context == null || context.Pipeline == null)
            return;

        var action = context.Pipeline.Option.Execution.DuplicateResourceId.GetValueOrDefault(ExecutionOption.Default.DuplicateResourceId.Value);
        context.Throw(action, PSRuleResources.DuplicateResourceId, id.Value, duplicateId.Value);
    }

    internal static void SuppressionGroupExpired(this RunspaceContext context, ResourceId suppressionGroupId)
    {
        if (context == null || context.Pipeline == null)
            return;

        var action = context.Pipeline.Option.Execution.SuppressionGroupExpired.GetValueOrDefault(ExecutionOption.Default.SuppressionGroupExpired.Value);
        context.Throw(action, PSRuleResources.SuppressionGroupExpired, suppressionGroupId.Value);
    }

    internal static void RuleExcluded(this RunspaceContext context, ResourceId ruleId)
    {
        if (context == null || context.Pipeline == null)
            return;

        var action = context.Pipeline.Option.Execution.RuleExcluded.GetValueOrDefault(ExecutionOption.Default.RuleExcluded.Value);
        context.Throw(action, PSRuleResources.RuleExcluded, ruleId.Value);
    }

    internal static void Throw(this RunspaceContext context, ExecutionActionPreference action, string message, params object[] args)
    {
        if (context == null || action == ExecutionActionPreference.Ignore)
            return;

        if (action == ExecutionActionPreference.Error)
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, message, args));

        else if (action == ExecutionActionPreference.Warn && context.Writer != null && context.Writer.ShouldWriteWarning())
            context.Writer.WriteWarning(message, args);

        else if (action == ExecutionActionPreference.Debug && context.Writer != null && context.Writer.ShouldWriteDebug())
            context.Writer.WriteDebug(message, args);
    }

    internal static void DebugPropertyObsolete(this RunspaceContext context, string variableName, string propertyName)
    {
        if (context == null || context.Writer == null || !context.Writer.ShouldWriteDebug())
            return;

        context.Writer.WriteDebug(PSRuleResources.DebugPropertyObsolete, context.RuleBlock.Name, variableName, propertyName);
    }

    internal static void WarnAliasReference(this RunspaceContext context, ResourceKind kind, string resourceId, string targetId, string alias)
    {
        var action = context.Pipeline.Option.Execution.AliasReference.GetValueOrDefault(ExecutionOption.Default.AliasReference.Value);
        Throw(context, action, PSRuleResources.AliasReference, kind.ToString(), resourceId, targetId, alias);
    }

    internal static void WarnAliasSuppression(this RunspaceContext context, string targetId, string alias)
    {
        var action = context.Pipeline.Option.Execution.AliasReference.GetValueOrDefault(ExecutionOption.Default.AliasReference.Value);
        Throw(context, action, PSRuleResources.AliasSuppression, targetId, alias);
    }
}
