using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, "Exists")]
    internal sealed class AssertExistsCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Field { get; set; }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            var result = PSObject.AsPSObject(inputObject).Properties.FirstOrDefault(p => p.Name == Field) != null;

            WriteObject(result);
        }
    }
}
