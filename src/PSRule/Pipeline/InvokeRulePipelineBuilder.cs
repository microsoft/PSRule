using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct an invoke pipeline.
    /// </summary>
    public sealed class InvokeRulePipelineBuilder
    {
        private RuleSource[] _Source;
        private PSRuleOption _Option;
        private Hashtable _Tag;
        private RuleOutcome _Outcome;
        private PipelineLogger _Logger;
        private ResultFormat _ResultFormat;
        private BindTargetName _BindTargetNameHook;
        private VisitTargetObject _VisitTargetObject;
        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;
        private Action<object, bool> _Output;
        private bool _ReturnBoolean;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
            _Outcome = RuleOutcome.Processed;
            _ResultFormat = ResultFormat.Detail;
            _BindTargetNameHook = PipelineHookActions.DefaultTargetNameBinding;
            _VisitTargetObject = PipelineReceiverActions.PassThru;
            _LogError = _LogWarning = _LogVerbose = _LogInformation = false;
            _Output = (r, b) => { };
        }

        public void FilterBy(Hashtable tag)
        {
            _Tag = tag;
        }

        public void Source(RuleSource[] source)
        {
            _Source = source;
        }

        public void Limit(RuleOutcome outcome)
        {
            _Outcome = outcome;
        }

        public void As(ResultFormat resultFormat)
        {
            _ResultFormat = resultFormat;
        }

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.OnWriteVerbose = commandRuntime.WriteVerbose;
            _Logger.OnWriteWarning = commandRuntime.WriteWarning;
            _Logger.OnWriteError = commandRuntime.WriteError;
            _Logger.OnWriteInformation = commandRuntime.WriteInformation;
            _Output = commandRuntime.WriteObject;
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

        private void AddVisitTargetObjectAction(VisitTargetObjectAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _VisitTargetObject;
            _VisitTargetObject = (targetObject) => action(targetObject, previous);
        }

        public void ReturnBoolean()
        {
            _ReturnBoolean = true;
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

            _Option.Input.Format = option.Input.Format ?? InputOption.Default.Format;
            _Option.Input.ObjectPath = option.Input.ObjectPath ?? InputOption.Default.ObjectPath;

            if (option.Baseline != null)
            {
                _Option.Baseline = new BaselineOption(option.Baseline);
            }

            if (option.Binding.TargetName != null && option.Binding.TargetName.Length > 0)
            {
                // Use nested TargetName binding when '.' is included in field name because it's slower then custom
                var useNested = option.Binding.TargetName.Any(n => n.Contains('.'));

                if (useNested)
                {
                    AddBindTargetNameAction((targetObject, next) =>
                    {
                        return PipelineHookActions.NestedTargetNameBinding(option.Binding.TargetName, targetObject, next);
                    });
                }
                else
                {
                    AddBindTargetNameAction((targetObject, next) =>
                    {
                        return PipelineHookActions.CustomTargetNameBinding(option.Binding.TargetName, targetObject, next);
                    });
                }
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
            if (!string.IsNullOrEmpty(_Option.Input.ObjectPath))
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ReadObjectPath(sourceObject, next, _Option.Input.ObjectPath, true);
                });
            }

            if (_Option.Input.Format == InputFormat.Yaml)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromYaml(sourceObject, next);
                });
            }
            else if (_Option.Input.Format == InputFormat.Json)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromJson(sourceObject, next);
                });
            }

            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: _Tag, exclude: _Option.Baseline.Exclude);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: _BindTargetNameHook, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose, logInformation: _LogInformation);
            var pipeline = new InvokeRulePipeline(
                stream: new PipelineStream(input: _VisitTargetObject, output: _Output),
                option: _Option,
                source: _Source,
                filter: filter,
                outcome: _Outcome,
                resultFormat: _ResultFormat,
                context: context,
                returnBoolean: _ReturnBoolean
            );

            return pipeline;
        }
    }
}
