using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Exists keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Exists)]
    internal sealed class AssertExistsCommand : RuleKeyword
    {
        public AssertExistsCommand()
        {
            CaseSensitive = false;
            Not = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string[] Field { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Not { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            var targetObject = InputObject ?? GetTargetObject();

            bool expected = !Not;
            bool actual = Not;

            for (var i = 0; i < Field.Length && actual != expected; i++)
            {
                actual = ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: targetObject, name: Field[i], caseSensitive: CaseSensitive, value: out object fieldValue);

                if (actual)
                {
                    PipelineContext.CurrentThread.VerboseConditionMessage(condition: RuleLanguageNouns.Exists, message: PSRuleResources.ExistsTrue, args: Field[i]);
                }
            }

            var result = expected == actual;
            PipelineContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Exists, outcome: result);
            WriteObject(expected == actual);
        }
    }
}
