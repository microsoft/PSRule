using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        internal GetRulePipeline(Source[] source, PipelineContext context)
            : base(context, source)
        {
            // Do nothing
        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(source: _Source, context: _Context);
        }
    }
}
