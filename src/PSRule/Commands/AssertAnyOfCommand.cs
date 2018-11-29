using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The AnyOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AnyOf)]
    internal sealed class AssertAnyOfCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            //WriteVerbose("[Rule][$($Context.Index)][$($Rule.Name)][AnyOf]::BEGIN");

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

                if (successCount >= 1)
                {
                    result = true;
                }

                //WriteVerbose($"[Rule][$($Context.Index)][$($Rule.Name)][AnyOf] -- [{successCount}/{totalCount}]");

                WriteObject(result);
            }
            finally
            {
                //WriteVerbose($"[Rule][$($Context.Index)][$($Rule.Name)][AnyOf]::END [{result}]");
            }
        }
    }
}
