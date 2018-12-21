using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct an invoke pipeline.
    /// </summary>
    public sealed class InvokeRulePipelineBuilder
    {
        private string[] _Path;
        private PSRuleOption _Option;
        private RuleFilter _Filter;
        private RuleOutcome _Outcome;
        private PipelineLogger _Logger;
        private ResultFormat _ResultFormat;
        private BindTargetName _BindTargetNameHook;
        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
            _ResultFormat = ResultFormat.Detail;
            _BindTargetNameHook = PipelineHookActions.DefaultBindTargetName;
        }

        public void FilterBy(string[] ruleName, Hashtable tag)
        {
            _Filter = new RuleFilter(ruleName, tag);
        }

        public void Source(string[] path)
        {
            _Path = path;
        }

        public void Option(PSRuleOption option)
        {
            _Option = option.Clone();
        }

        public void Limit(RuleOutcome outcome)
        {
            _Outcome = outcome;
        }

        public void As(ResultFormat resultFormat)
        {
            _ResultFormat = resultFormat;
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
        }

        public void UseLoggingPreferences(ActionPreference error, ActionPreference warning, ActionPreference verbose)
        {
            _LogError = !(error == ActionPreference.Ignore || error == ActionPreference.SilentlyContinue);
            _LogWarning = !(warning == ActionPreference.Ignore || warning == ActionPreference.SilentlyContinue);
            _LogVerbose = !(verbose == ActionPreference.Ignore || verbose == ActionPreference.SilentlyContinue);
        }

        public void AddBindTargetNameAction(BindTargetNameAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _BindTargetNameHook;
            _BindTargetNameHook = (targetObject) => action(targetObject, previous);
        }

        public InvokeRulePipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
            {
                return this;
            }

            if (option.Pipeline.BindTargetName.Count > 0)
            {
                foreach (var action in option.Pipeline.BindTargetName)
                {
                    AddBindTargetNameAction((command, next) =>
                    {
                        action(command);

                        return next(command);
                    });
                }
            }

            return this;
        }

        public InvokeRulePipeline Build()
        {
            var context = PipelineContext.New(_Logger, _BindTargetNameHook, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose);

            return new InvokeRulePipeline(_Option, _Path, _Filter, _Outcome, _ResultFormat, context: context);
        }
    }
}
