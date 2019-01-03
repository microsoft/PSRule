﻿using PSRule.Configuration;
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
        private bool _LogInformation;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
            _Outcome = RuleOutcome.Processed;
            _ResultFormat = ResultFormat.Detail;
            _BindTargetNameHook = PipelineHookActions.DefaultTargetNameBinding;
            _LogError = _LogWarning = _LogVerbose = _LogInformation = false;
        }

        public void FilterBy(string[] ruleName, Hashtable tag)
        {
            _Filter = new RuleFilter(ruleName, tag);
        }

        public void Source(string[] path)
        {
            _Path = path;
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
            _Logger.OnWriteInformation = commandRuntime.WriteInformation;
        }

        public void UseLoggingPreferences(ActionPreference error, ActionPreference warning, ActionPreference verbose, ActionPreference information)
        {
            _LogError = !(error == ActionPreference.Ignore);
            _LogWarning = !(warning == ActionPreference.Ignore);
            _LogVerbose = !(verbose == ActionPreference.Ignore || verbose == ActionPreference.SilentlyContinue);
            _LogInformation = !(information == ActionPreference.Ignore || information == ActionPreference.SilentlyContinue);
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

            _Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            _Option.Execution.InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning;
            _Option.Execution.NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning;

            if (option.Binding.TargetName != null && option.Binding.TargetName.Length > 0)
            {
                AddBindTargetNameAction((targetObject, next) =>
                {
                    return PipelineHookActions.CustomTargetNameBinding(option.Binding.TargetName, targetObject, next);
                });
            }

            if (option.Pipeline.BindTargetName != null && option.Pipeline.BindTargetName.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (_Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                {
                    throw new PipelineConfigurationException(optionName: "BindTargetName", message: "Binding functions are not supported in this language mode.");
                }

                foreach (var action in option.Pipeline.BindTargetName)
                {
                    AddBindTargetNameAction((targetObject, next) =>
                    {
                        var targetName = action(targetObject);

                        return string.IsNullOrEmpty(targetName) ? next(targetObject) : targetName;
                    });
                }
            }

            if (option.Suppression.Count > 0)
            {
                _Option.Suppression = new SuppressionOption(option.Suppression);
            }

            return this;
        }

        public InvokeRulePipeline Build()
        {
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: _BindTargetNameHook, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose, logInformation: _LogInformation);
            return new InvokeRulePipeline(_Option, _Path, _Filter, _Outcome, _ResultFormat, context: context);
        }
    }
}
