using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSRule.Commands
{
    [Cmdlet(VerbsCommon.Set, "PSRuleHint")]
    internal sealed class SetPSRuleHintCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = false)]
        public string TargetName { get; set; }

        protected override void ProcessRecord()
        {
            var result = GetResult();

            result.TargetName = TargetName;

            if (MyInvocation.BoundParameters.ContainsKey("TargetName"))
            {
                result.TargetName = TargetName;
            }
        }
    }
}
