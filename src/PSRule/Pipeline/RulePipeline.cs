using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public abstract class RulePipeline
    {
        protected readonly string[] _Path;
        protected readonly RuleFilter _Filter;
        protected readonly LanguageContext _Context;

        internal RulePipeline(string[] path, RuleFilter filter)
        {
            _Path = path;
            _Filter = filter;
            _Context = new LanguageContext();
        }
    }
}