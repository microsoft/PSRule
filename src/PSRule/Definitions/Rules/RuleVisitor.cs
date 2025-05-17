// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Rules;

/// <summary>
/// A rule visitor.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
internal sealed class RuleVisitor : ICondition
{
    private readonly LanguageExpressionOuterFn _Condition;
    private readonly LegacyRunspaceContext _Context;

    public RuleVisitor(LegacyRunspaceContext context, ResourceId id, ISourceFile source, IRuleSpec spec)
    {
        _Context = context;
        ErrorAction = ActionPreference.Stop;
        Id = id;
        Source = source;
        InstanceId = Guid.NewGuid();
        var builder = new LanguageExpressionBuilder(id);
        _Condition = builder
            .WithSelector(spec.With)
            .WithType(spec.Type)
            .WithSubselector(spec.Where)
            .Build(spec.Condition);
    }

    public Guid InstanceId { get; }

    public ISourceFile Source { get; }

    public ResourceId Id { get; }

    public ActionPreference ErrorAction { get; }

    public void Dispose()
    {
        // Do nothing
    }

    public IConditionResult If()
    {
        var context = new ExpressionContext(_Context, Source, ResourceKind.Rule, _Context.TargetObject);
        context.Debug(PSRuleResources.RuleMatchTrace, Id);
        context.PushScope(RunspaceScope.Rule);
        try
        {
            var result = _Condition(context, _Context.TargetObject);
            if (result.HasValue && !result.Value)
                _Context.WriteReason(context.GetReasons());

            return result.HasValue ? new RuleConditionResult(result.Value ? 1 : 0, 1, false) : null;
        }
        finally
        {
            context.PopScope(RunspaceScope.Rule);
        }
    }
}
