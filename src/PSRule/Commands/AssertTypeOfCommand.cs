using System.Linq;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The TypeOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.TypeOf)]
    internal sealed class AssertTypeOfCommand : RuleKeyword
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
