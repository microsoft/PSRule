using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AllOf)]
    internal sealed class AssertAllOfCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            var result = false;

            var invokeResult = Body.Invoke();
            var totalCount = 0;
            var successCount = 0;

            foreach (var ir in invokeResult)
            {
                if (ir.BaseObject is bool)
                {
                    totalCount++;

                    if ((bool)ir.BaseObject)
                    {
                        successCount++;
                    }
                }
            }

            if (successCount == totalCount)
            {
                result = true;
            }

            WriteObject(result);
        }
    }
}
