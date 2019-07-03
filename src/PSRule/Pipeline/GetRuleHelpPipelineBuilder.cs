using PSRule.Configuration;
using PSRule.Rules;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class GetRuleHelpPipelineBuilder
    {
        private readonly PSRuleOption _Option;
        private readonly PipelineLogger _Logger;

        private RuleSource[] _Source;

        internal GetRuleHelpPipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
        }

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

            _Logger.Configure(_Option);
            return this;
        }

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _Logger.UseExecutionContext(executionContext);
        }

        public GetRuleHelpPipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: null, exclude: _Option.Baseline.Exclude, wildcardMatch: true);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: null, bindTargetType: null);
            return new GetRuleHelpPipeline(option: _Option, source: _Source, filter: filter, context: context);
        }
    }
}
