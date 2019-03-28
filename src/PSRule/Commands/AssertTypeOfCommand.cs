using PSRule.Pipeline;
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

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            var inputObject = InputObject ?? GetTargetObject();
            var result = false;

            if (inputObject != null)
            {
                var actualTypeNames = PSObject.AsPSObject(inputObject).TypeNames.ToArray();

                if (actualTypeNames.Intersect(TypeName).Count() > 0)
                {
                    result = true;
                }
            }

            PipelineContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.TypeOf, outcome: result);

            WriteObject(result);
        }
    }
}
