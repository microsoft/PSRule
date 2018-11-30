using PSRule.Pipeline;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Exists keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Exists)]
    internal sealed class AssertExistsCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string[] Field { get; set; }

        protected override void ProcessRecord()
        {
            PipelineContext.WriteVerbose("[Exists]::BEGIN");

            try
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
            finally
            {
                PipelineContext.WriteVerbose("[Exists]::END");
            }
        }
    }
}
