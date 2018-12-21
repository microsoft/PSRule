using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        private readonly PipelineContext _Context;

        internal GetRulePipeline(PSRuleOption option, string[] path, RuleFilter filter, PipelineContext context)
            : base(option, path, filter)
        {
            _Context = context;
        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(_Option, _Path, _Filter);
        }
    }
}
