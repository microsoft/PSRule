using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    public sealed class GetRulePipelineBuilder
    {
        private string[] _Path;
        private PSRuleOption _Option;
        private Hashtable _Tag;
        private PipelineLogger _Logger;
        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;

        internal GetRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
        }

        public void FilterBy(Hashtable tag)
        {
            _Tag = tag;
        }

        public void Source(string[] path)
        {
            _Path = path;
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

            return this;
        }

        public void UseCommandRuntime(ICommandRuntime commandRuntime)
        {
            _Logger.OnWriteVerbose = commandRuntime.WriteVerbose;
            _Logger.OnWriteWarning = commandRuntime.WriteWarning;
            _Logger.OnWriteError = commandRuntime.WriteError;
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
            _LogError = !(error == ActionPreference.Ignore);
            _LogWarning = !(warning == ActionPreference.Ignore);
            _LogVerbose = !(verbose == ActionPreference.Ignore || verbose == ActionPreference.SilentlyContinue);
            _LogInformation = !(information == ActionPreference.Ignore || information == ActionPreference.SilentlyContinue);
        }

        public GetRulePipeline Build()
        {
            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: _Tag, exclude: _Option.Baseline.Exclude);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: null, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose, logInformation: _LogInformation);
            return new GetRulePipeline(option: _Option, path: _Path, filter: filter, context: context);
        }
    }
}
