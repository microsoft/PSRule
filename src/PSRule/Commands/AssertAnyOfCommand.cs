using PSRule.Pipeline;
using PSRule.Rules;
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
            var invokeResult = RuleConditionResult.Create(Body.Invoke());

            var result = invokeResult.AnyOf();

            PipelineContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.AnyOf, pass: invokeResult.Pass, count: invokeResult.Count, outcome: result);

            WriteObject(result);
        }
    }
}
