using PSRule.Pipeline;
using PSRule.Rules;
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
            if (Body == null)
            {
                return;
            }

            // Evalute type pre-condition
            if (Type != null)
            {
                var comparer = PipelineContext.CurrentThread.Option.Binding.IgnoreCase.Value ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

                if (!Type.Contains(value: PipelineContext.CurrentThread.TargetType, comparer: comparer))
                {
                    PipelineContext.CurrentThread.Logger.DebugMessage("Target failed Type precondition");
                    return;
                }
            }

            // Evaluate script pre-condition
            if (If != null)
            {
                var ifResult = If.InvokeReturnAsIs() as PSObject;

                if (ifResult == null || !(ifResult.BaseObject is bool) || !(bool)ifResult.BaseObject)
                {
                    PipelineContext.CurrentThread.Logger.DebugMessage("Target failed If precondition");
                    return;
                }
            }

            // Evaluate script block
            var invokeResult = RuleConditionResult.Create(Body.Invoke());

            WriteObject(invokeResult);
        }
    }
}