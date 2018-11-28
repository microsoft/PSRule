using System.Linq;
using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, InternalCommandVerbs.TypeOf)]
    internal sealed class AssertTypeOfCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string[] TypeName { get; set; }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            var result = false;

            if (inputObject != null)
            {
                var actualTypeNames = PSObject.AsPSObject(inputObject).TypeNames.ToArray();

                if (actualTypeNames.Intersect(TypeName).Count() > 0)
                {
                    result = true;
                }
            }

            WriteObject(result);
        }
    }
}
