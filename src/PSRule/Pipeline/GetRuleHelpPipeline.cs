using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRuleHelpPipeline : RulePipeline
    {
        internal GetRuleHelpPipeline(Source[] source, PipelineContext context)
            : base(context, source)
        {
            // Do nothing
        }

        public IEnumerable<RuleHelpInfo> Process()
        {
            return HostHelper.GetRuleHelp(source: _Source, _Context);
        }
    }
}
