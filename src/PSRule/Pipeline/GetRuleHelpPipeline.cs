using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class GetRuleHelpPipeline : RulePipeline
    {
        internal GetRuleHelpPipeline(PSRuleOption option, RuleSource[] source, RuleFilter filter, PipelineContext context)
            : base(context, option, source, filter)
        {
            // Do nothing
        }

        public RuleHelpInfo Process()
        {
            return HostHelper.GetRuleHelp(source: _Source, filter: _Filter);
        }
    }
}
