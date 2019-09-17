using PSRule.Configuration;
using System.Threading;

namespace PSRule.Pipeline
{
    public sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase
    {
        public GetRuleHelpPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
            {
                return this;
            }

            _Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            _Option.Output.Culture = option.Output.Culture ?? new string[] { Thread.CurrentThread.CurrentCulture.ToString() };

            if (option.Rule != null)
            {
                _Option.Rule = new RuleOption(option.Rule);
            }

            ConfigureLogger(_Option);
            return this;
        }

        public GetRuleHelpPipeline Build()
        {
            var context = PrepareContext(bindTargetName: null, bindTargetType: null);
            return new GetRuleHelpPipeline(source: GetSource(), context: context);
        }
    }
}
