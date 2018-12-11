using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        private readonly PipelineContext _Context;

        internal GetRulePipeline(PipelineLogger logger, PSRuleOption option, string[] path, RuleFilter filter)
            : base(option, path, filter)
        {
            _Context = PipelineContext.New(logger);
        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(_Option, null, _Path, _Filter);
        }
    }
}
