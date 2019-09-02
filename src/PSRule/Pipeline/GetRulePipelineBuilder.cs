using PSRule.Configuration;
using System.Threading;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    public sealed class GetRulePipelineBuilder : PipelineBuilderBase
    {
        public GetRulePipelineBuilder Configure(PSRuleOption option)
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

        public GetRulePipeline Build()
        {
            var context = PrepareContext(bindTargetName: null, bindTargetType: null);
            return new GetRulePipeline(source: GetSource(), context: context);
        }
    }
}
