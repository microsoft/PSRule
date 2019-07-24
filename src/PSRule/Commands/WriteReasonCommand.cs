using PSRule.Pipeline;
using PSRule.Resources;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Reason keyword.
    /// </summary>
    [Cmdlet(VerbsCommunications.Write, RuleLanguageNouns.Reason)]
    internal sealed class WriteReasonCommand : RuleKeyword
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Text { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsConditionScope())
            {
                throw new RuleRuntimeException(string.Format(PSRuleResources.KeywordConditionScope, LanguageKeywords.Reason));
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Text)))
            {
                PipelineContext.CurrentThread.WriteReason(text: Text);
            }
        }
    }
}
