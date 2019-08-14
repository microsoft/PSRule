using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase
    {
        private RuleSource[] _Source;

        public void Source(RuleSource[] source)
        {
            _Source = source;
        }

        public GetRuleHelpPipelineBuilder Configure(PSRuleOption option)
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

        public GetRuleHelpPipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: null, exclude: _Option.Baseline.Exclude, wildcardMatch: true);
            var context = PrepareContext(bindTargetName: null, bindTargetType: null);
            return new GetRuleHelpPipeline(option: _Option, source: _Source, filter: filter, context: context);
        }
    }
}
