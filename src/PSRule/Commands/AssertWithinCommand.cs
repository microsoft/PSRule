using System;
using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, "Within")]
    internal sealed class AssertWithinCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Field { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            var result = false;

            var invokeResult = Body.Invoke();

            if (GetField(inputObject, Field, out object fieldValue))
            {
                foreach (var ir in invokeResult)
                {
                    if (fieldValue is string && ir.BaseObject is string)
                    {
                        if (StringComparer.OrdinalIgnoreCase.Equals(fieldValue, ir.BaseObject))
                        {
                            result = true;
                        }
                    }
                    else if (ir == fieldValue)
                    {
                        result = true;
                    }
                }
            }

            WriteObject(result);
        }
    }
}
