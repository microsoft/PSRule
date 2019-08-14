using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    public sealed class GetRulePipelineBuilder : PipelineBuilderBase
    {
        private RuleSource[] _Source;
        private Hashtable _Tag;

        public void FilterBy(Hashtable tag)
        {
            _Tag = tag;
        }

        public void Source(RuleSource[] source)
        {
            _Source = source;
        }

        public GetRulePipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
            {
                return this;
            }

            _Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;

            if (option.Baseline != null)
            {
                _Option.Baseline.RuleName = option.Baseline.RuleName;
                _Option.Baseline.Exclude = option.Baseline.Exclude;
            }

            ConfigureLogger(_Option);
            return this;
        }

        public GetRulePipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: _Tag, exclude: _Option.Baseline.Exclude);
            var context = PrepareContext(bindTargetName: null, bindTargetType: null);
            return new GetRulePipeline(option: _Option, source: _Source, filter: filter, context: context);
        }
    }
}
