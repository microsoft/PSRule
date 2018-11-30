using PSRule.Pipeline;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The AllOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AllOf)]
    internal sealed class AssertAllOfCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            PipelineContext.WriteVerbose("[AllOf]::BEGIN");

            var result = false;

            try
            {
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
            finally
            {
                PipelineContext.WriteVerbose($"[AllOf]::END [{result}]");
            }
        }
    }
}
