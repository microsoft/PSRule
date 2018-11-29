using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public interface IRulePipeline
    {
        IEnumerable<Rule> Process(string[] path, RuleFilter filter);
    }
}
