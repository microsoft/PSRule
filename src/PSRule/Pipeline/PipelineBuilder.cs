using PSRule.Configuration;
using PSRule.Rules;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static InvokeRulePipelineBuilder Invoke()
        {
            return new InvokeRulePipelineBuilder();
        }

        public static GetRulePipelineBuilder Get()
        {
            return new GetRulePipelineBuilder();
        }

        public static GetRuleHelpPipelineBuilder GetHelp()
        {
            return new GetRuleHelpPipelineBuilder();
        }

        public static RuleSourceBuilder RuleSource()
        {
            return new RuleSourceBuilder();
        }
    }

    public abstract class PipelineBuilderBase
    {
        private readonly PipelineLogger _Logger;
        protected readonly PSRuleOption _Option;

        protected PipelineBuilderBase()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
        }

        public virtual void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _Logger.UseExecutionContext(executionContext);
        }

        protected void ConfigureLogger(PSRuleOption option)
        {
            _Logger.Configure(option);
        }

        internal PipelineContext PrepareContext(BindTargetName bindTargetName, BindTargetName bindTargetType)
        {
            return PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: bindTargetName, bindTargetType: bindTargetType);
        }
    }
}
