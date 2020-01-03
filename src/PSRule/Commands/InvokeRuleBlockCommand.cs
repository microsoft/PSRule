// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;
using System;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// An internal langauge command used to evaluate a rule script block.
    /// </summary>
    internal sealed class InvokeRuleBlockCommand : Cmdlet
    {
        [Parameter()]
        public string[] Type;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        protected override void ProcessRecord()
        {
            try
            {
                if (Body == null)
                    return;

                // Evalute type pre-condition
                if (Type != null)
                {
                    var comparer = PipelineContext.CurrentThread.Baseline.GetTargetBinding().IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    if (!Type.Contains(value: PipelineContext.CurrentThread.RuleRecord.TargetType, comparer: comparer))
                    {
                        PipelineContext.CurrentThread.Logger.DebugMessage("Target failed Type precondition");
                        return;
                    }
                }

                // Evaluate script pre-condition
                if (If != null)
                {
                    PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Precondition;
                    var ifResult = RuleConditionResult.Create(If.Invoke());
                    if (!ifResult.AllOf())
                    {
                        PipelineContext.CurrentThread.Logger.DebugMessage("Target failed If precondition");
                        return;
                    }
                }

                // Evaluate script block
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Condition;
                var invokeResult = RuleConditionResult.Create(Body.Invoke());

                WriteObject(invokeResult);
            }
            finally
            {
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.None;
            }
        }
    }
}