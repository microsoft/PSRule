using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public abstract class RulePipeline
    {
        protected readonly PSRuleOption _Option;
        protected readonly string[] _Path;
        protected readonly RuleFilter _Filter;
        protected readonly LanguageContext _Context;

        internal RulePipeline(PSRuleOption option, string[] path, RuleFilter filter)
        {
            _Option = option;
            _Path = path;
            _Filter = filter;
            _Context = new LanguageContext();
        }
    }
}
