// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Rules;

/// <summary>
/// Define a condition implemented as meta language expressions.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
internal sealed class RuleVisitor(ResourceId id, ISourceFile source, IRuleSpec spec) : ICondition
{
    private readonly LanguageExpressionOuterFn _Condition = new LanguageExpressionBuilder(id)
            .WithSelector(spec.With)
            .WithType(spec.Type)
            .WithSubselector(spec.Where)
            .Build(spec.Condition);

    public Guid InstanceId { get; } = Guid.NewGuid();

    public ISourceFile Source { get; } = source;

    public ResourceId Id { get; } = id;

    public ActionPreference ErrorAction { get; } = ActionPreference.Stop;

    public void Dispose()
    {
        // Do nothing
    }

    public IConditionResult? If(IExpressionContext expressionContext, ITargetObject o)
    {
        var context = new ExpressionContext(expressionContext, Source, ResourceKind.Rule, o);
        context.Logger.LogDebug(EventId.None, PSRuleResources.RuleMatchTrace, Id);
        context.PushScope(RunspaceScope.Rule);
        try
        {
            var result = _Condition(context, o);
            if (result.HasValue && !result.Value)
            {
                foreach (var reason in context.GetReasons())
                {
                    expressionContext.Reason(reason.Operand, reason.Text, reason.Args);
                }
            }
            return result.HasValue ? new RuleConditionResult(result.Value ? 1 : 0, 1, false) : null;
        }
        finally
        {
            context.PopScope(RunspaceScope.Rule);
        }
    }
}
