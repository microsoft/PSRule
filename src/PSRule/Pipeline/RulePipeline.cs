using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public abstract class RulePipeline
    {
        protected readonly PSRuleOption _Option;
        protected readonly string[] _Path;
        protected readonly RuleFilter _Filter;

        internal RulePipeline(PSRuleOption option, string[] path, RuleFilter filter)
        {
            _Option = option;
            _Path = path;
            _Filter = filter;
        }
    }
}
