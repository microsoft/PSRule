using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        internal GetRulePipeline(string[] path, RuleFilter filter)
            : base(path, filter)
        {

        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(_Context, _Path, _Filter);
        }
    }
}
