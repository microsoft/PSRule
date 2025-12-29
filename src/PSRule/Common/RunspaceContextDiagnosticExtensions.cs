// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Pipeline.Runs;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

internal static class RunspaceContextDiagnosticExtensions
{
    private const string WARN_KEY_PROPERTY = "Property";

    internal static void WarnPropertyObsolete(this LegacyRunspaceContext context, string variableName, string propertyName)
    {
        context.DebugPropertyObsolete(variableName, propertyName);
        if (context.Writer == null || !context.Writer.IsEnabled(LogLevel.Warning) || !context.ShouldWarnOnce(WARN_KEY_PROPERTY, variableName, propertyName))
            return;

        context.Writer.WriteWarning(PSRuleResources.PropertyObsolete, variableName, propertyName);
    }

    /// <summary>
    /// The option '{0}' is deprecated and will be removed with PSRule v3. See http://aka.ms/ps-rule/deprecations for more detail.
    /// </summary>
    internal static void WarnDeprecatedOption(this LegacyRunspaceContext context, string option)
    {
        if (context.Writer == null || !context.Writer.IsEnabled(LogLevel.Warning))
            return;

        context.Writer.WriteWarning(PSRuleResources.DeprecatedOption, option);
    }

    internal static void DuplicateResourceName(this IResourceContext context, ResourceId id, string name)
    {
        context.ReportIssue(new ResourceIssue(ResourceIssueType.DuplicateResourceName, id, name));
    }

    internal static void DuplicateResourceId(this IResourceContext context, ResourceId id, ResourceId duplicateId)
    {
        context.ReportIssue(new ResourceIssue(ResourceIssueType.DuplicateResourceId, id, duplicateId.Value));
    }

    internal static void SuppressionGroupExpired(this LegacyRunspaceContext context, ResourceId suppressionGroupId)
    {
        if (context == null || context.Pipeline == null)
            return;

        var action = context.Pipeline.Option.Execution.SuppressionGroupExpired.GetValueOrDefault(ExecutionOption.Default.SuppressionGroupExpired.Value);
        context.Throw(action, PSRuleResources.SuppressionGroupExpired, suppressionGroupId.Value);
    }

    internal static void RuleExcluded(this IRunBuilderContext context, ResourceId ruleId)
    {
        if (context == null)
            return;

        context.ReportIssue(new ResourceIssue(ResourceIssueType.RuleExcluded, ruleId));
    }

    internal static void Throw(this LegacyRunspaceContext context, ExecutionActionPreference action, string message, params object[] args)
    {
        if (context == null || action == ExecutionActionPreference.Ignore)
            return;

        if (action == ExecutionActionPreference.Error)
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, message, args));

        else if (action == ExecutionActionPreference.Warn && context.Writer != null && context.Writer.IsEnabled(LogLevel.Warning))
            context.Writer.WriteWarning(message, args);

        else if (action == ExecutionActionPreference.Debug && context.Writer != null && context.Writer.IsEnabled(LogLevel.Debug))
            context.Writer.WriteDebug(message, args);
    }

    internal static void DebugPropertyObsolete(this LegacyRunspaceContext context, string variableName, string propertyName)
    {
        if (context == null || context.Writer == null || !context.Writer.IsEnabled(LogLevel.Debug))
            return;

        context.Writer.WriteDebug(PSRuleResources.DebugPropertyObsolete, context.RuleBlock.Name, variableName, propertyName);
    }

    internal static void WarnAliasReference(this IRunBuilderContext context, ResourceId id, ResourceId targetId, ResourceKind kind, string alias)
    {
        if (context == null) return;

        context.ReportIssue(new ResourceIssue(ResourceIssueType.AliasReference, id, targetId.Value, kind.ToString(), alias));
    }

    internal static void WarnAliasSuppression(this LegacyRunspaceContext context, string targetId, string alias)
    {
        var action = context.Pipeline.Option.Execution.AliasReference.GetValueOrDefault(ExecutionOption.Default.AliasReference.Value);
        Throw(context, action, PSRuleResources.AliasSuppression, targetId, alias);
    }
}
