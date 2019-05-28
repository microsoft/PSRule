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
        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;

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

            return this;
        }

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.OnWriteVerbose = commandRuntime.WriteVerbose;
            _Logger.OnWriteWarning = commandRuntime.WriteWarning;
            _Logger.OnWriteError = commandRuntime.WriteError;
            _Logger.OnWriteInformation = commandRuntime.WriteInformation;
        }

        public void UseLoggingPreferences(ActionPreference error, ActionPreference warning, ActionPreference verbose, ActionPreference information)
        {
            _LogError = (error != ActionPreference.Ignore);
            _LogWarning = (warning != ActionPreference.Ignore);
            _LogVerbose = !(verbose == ActionPreference.Ignore || verbose == ActionPreference.SilentlyContinue);
            _LogInformation = !(information == ActionPreference.Ignore || information == ActionPreference.SilentlyContinue);
        }

        public GetRuleHelpPipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: null, exclude: _Option.Baseline.Exclude);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: null, bindTargetType: null, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose, logInformation: _LogInformation);
            return new GetRuleHelpPipeline(option: _Option, source: _Source, filter: filter, context: context);
        }
    }
}
