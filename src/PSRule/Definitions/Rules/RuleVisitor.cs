// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Diagnostics;

namespace PSRule.Definitions.Rules
{
    [DebuggerDisplay("Id: {Id}")]
    internal sealed class RuleVisitor : ICondition
    {
        private readonly LanguageExpressionOuterFn _Condition;

        public RuleVisitor(string id, IRuleSpec spec)
        {
            Id = id;
            InstanceId = Guid.NewGuid();
            var builder = new LanguageExpressionBuilder();
            _Condition = builder
                .WithSelector(spec.With)
                .WithType(spec.Type)
                .Build(spec.Condition);
        }

        public Guid InstanceId { get; }

        public string Id { get; }

        public void Dispose()
        {
            // Do nothing
        }

        public IConditionResult If()
        {
            var context = new ExpressionContext();
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            context.PushScope(RunspaceScope.Rule);
            try
            {
                var result = _Condition(context, RunspaceContext.CurrentThread.TargetObject.Value);
                if (result.HasValue && !result.Value)
                {
                    var reasons = context.GetReasons();
                    foreach (var reason in reasons)
                        RunspaceContext.CurrentThread.WriteReason(reason);
                }
                return result.HasValue ? new RuleConditionResult(result.Value ? 1 : 0, 1, false) : null;
            }
            finally
            {
                context.PopScope(RunspaceScope.Rule);
            }
        }
    }
}
