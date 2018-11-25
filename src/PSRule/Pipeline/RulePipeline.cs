using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    internal sealed class RulePipeline : IRulePipeline
    {
        public IEnumerable<Rule> Process(LanguageContext context, string[] path, RuleFilter filter)
        {
            return HostHelper.GetRule(context, path, filter);
        }
    }
}
