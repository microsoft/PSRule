using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    public sealed class GetRulePipelineBuilder
    {
        private readonly PSRuleOption _Option;
        private readonly PipelineLogger _Logger;

        private RuleSource[] _Source;
        private Hashtable _Tag;

        internal GetRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
        }

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

        public GetRulePipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: _Tag, exclude: _Option.Baseline.Exclude);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: null, bindTargetType: null);
            return new GetRulePipeline(option: _Option, source: _Source, filter: filter, context: context);
        }
    }
}
