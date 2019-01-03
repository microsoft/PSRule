using PSRule.Rules;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// An internal langauge command used to evaluate a rule script block.
    /// </summary>
    internal sealed class InvokeRuleBlockCommand : Cmdlet
    {
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

            // Evaluate pre-condition
            if (If != null)
            {
                var ifResult = If.InvokeReturnAsIs() as PSObject;

                if (ifResult == null || !(ifResult.BaseObject is bool) || !(bool)ifResult.BaseObject)
                {
                    return;
                }
            }

            // Evaluate script block
            var invokeResult = RuleConditionResult.Create(Body.Invoke());

            WriteObject(invokeResult);
        }
    }
}