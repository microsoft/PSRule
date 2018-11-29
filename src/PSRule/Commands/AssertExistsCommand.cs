using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Exists)]
    internal sealed class AssertExistsCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string[] Field { get; set; }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            bool result = false;

            foreach (var fieldName in Field)
            {
                if (GetField(inputObject, fieldName, out object fieldValue))
                {
                    result = true;
                }
            }

            WriteObject(result);
        }
    }
}
