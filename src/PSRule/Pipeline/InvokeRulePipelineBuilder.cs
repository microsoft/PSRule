using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct an invoke pipeline.
    /// </summary>
    public sealed class InvokeRulePipelineBuilder : PipelineBuilderBase
    {
        private RuleOutcome _Outcome;
        private BindTargetMethod _BindTargetNameHook;
        private BindTargetMethod _BindTargetTypeHook;
        private VisitTargetObject _VisitTargetObject;
        private ShouldProcess _ShouldProcess;
        private Action<object, bool> _Output;
        private bool _ReturnBoolean;
        private IPipelineStream _Stream;
        private string[] _InputPath;

        internal InvokeRulePipelineBuilder()
        {
            _Outcome = RuleOutcome.Processed;
            _BindTargetNameHook = PipelineHookActions.BindTargetName;
            _BindTargetTypeHook = PipelineHookActions.BindTargetType;
            _VisitTargetObject = PipelineReceiverActions.PassThru;
            _Output = (r, b) => { };
            _Stream = null;
            _InputPath = null;
        }

        public void Limit(RuleOutcome outcome)
        {
            _Outcome = outcome;
        }

        public override void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            base.UseCommandRuntime(commandRuntime);
            _ShouldProcess = commandRuntime.ShouldProcess;
            _Output = commandRuntime.WriteObject;
        }

        public void ReturnBoolean()
        {
            _ReturnBoolean = true;
        }

        public void InputPath(string[] path)
        {
            _InputPath = path;
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

            _Option.Logging.RuleFail = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail;
            _Option.Logging.RulePass = option.Logging.RulePass ?? LoggingOption.Default.RulePass;
            _Option.Logging.LimitVerbose = option.Logging.LimitVerbose;
            _Option.Logging.LimitDebug = option.Logging.LimitDebug;

            _Option.Output.As = option.Output.As ?? OutputOption.Default.As;
            _Option.Output.Culture = option.Output.Culture ?? new string[] { Thread.CurrentThread.CurrentCulture.ToString() };
            _Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
            _Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
            _Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;

            _Option.Binding.IgnoreCase = option.Binding.IgnoreCase ?? BindingOption.Default.IgnoreCase;
            _Option.Binding.TargetName = option.Binding.TargetName;
            _Option.Binding.TargetType = option.Binding.TargetType;

            if (option.Rule != null)
            {
                _Option.Rule = new RuleOption(option.Rule);
            }

            if (option.Configuration != null)
            {
                _Option.Configuration = new ConfigurationOption(option.Configuration);
            }

            //if (option.Binding.TargetName != null && option.Binding.TargetName.Length > 0)
            //{
            //    // Use nested TargetName binding when '.' is included in field name because it's slower then custom
            //    var useNested = option.Binding.TargetName.Any(n => n.Contains('.'));

            //    if (useNested)
            //        _BindTargetNameHook = AddBindTargetAction(PipelineHookActions.NestedTargetPropertyBinding, _BindTargetNameHook);
            //    else
            //        _BindTargetNameHook = AddBindTargetAction(PipelineHookActions.CustomTargetPropertyBinding, _BindTargetNameHook);
            //}

            if (option.Pipeline.BindTargetName != null && option.Pipeline.BindTargetName.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (_Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                {
                    throw new PipelineConfigurationException(optionName: "BindTargetName", message: PSRuleResources.ConstrainedTargetBinding);
                }

                foreach (var action in option.Pipeline.BindTargetName)
                {
                    _BindTargetNameHook = AddBindTargetAction(action, _BindTargetNameHook);
                }
            }

            //if (option.Binding.TargetType != null && option.Binding.TargetType.Length > 0)
            //{
            //    // Use nested TargetType binding when '.' is included in field name because it's slower then custom
            //    var useNested = option.Binding.TargetType.Any(n => n.Contains('.'));

            //    if (useNested)
            //        _BindTargetTypeHook = AddBindTargetAction(PipelineHookActions.NestedTargetPropertyBinding, _BindTargetTypeHook);
            //    else
            //        _BindTargetTypeHook = AddBindTargetAction(PipelineHookActions.CustomTargetPropertyBinding, _BindTargetTypeHook);
            //}

            if (option.Pipeline.BindTargetType != null && option.Pipeline.BindTargetType.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (_Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                {
                    throw new PipelineConfigurationException(optionName: "BindTargetType", message: PSRuleResources.ConstrainedTargetBinding);
                }

                foreach (var action in option.Pipeline.BindTargetType)
                {
                    _BindTargetTypeHook = AddBindTargetAction(action, _BindTargetTypeHook);
                }
            }

            if (option.Suppression.Count > 0)
            {
                _Option.Suppression = new SuppressionOption(option.Suppression);
            }

            ConfigureLogger(_Option);
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
            else if (_Option.Input.Format == InputFormat.Detect && _InputPath != null)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.DetectInputFormat(sourceObject, next);
                });
            }

            if (_Stream == null)
            {
                _Stream = new PowerShellPipelineStream(option: _Option, output: GetOutput(), returnBoolean: _ReturnBoolean, inputPath: _InputPath);
            }

            //var filter = new RuleFilter(include: _Option.Rule.Include, tag: _Tag, exclude: _Option.Rule.Exclude);
            var context = PrepareContext(bindTargetName: _BindTargetNameHook, bindTargetType: _BindTargetTypeHook);
            var pipeline = new InvokeRulePipeline(
                streamManager: new StreamManager(option: _Option, stream: _Stream, input: _VisitTargetObject),
                source: GetSource(),
                outcome: _Outcome,
                context: context
            );
            return pipeline;
        }

        private BindTargetMethod AddBindTargetAction(BindTargetFunc action, BindTargetMethod previous)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            return (propertyNames, caseSensitive, targetObject) => action(propertyNames, caseSensitive, targetObject, previous);
        }

        private BindTargetMethod AddBindTargetAction(BindTargetName action, BindTargetMethod previous)
        {
            return AddBindTargetAction((parameterNames, caseSensitive, targetObject, next) =>
            {
                var targetType = action(targetObject);
                return string.IsNullOrEmpty(targetType) ? next(parameterNames, caseSensitive, targetObject) : targetType;
            }, previous);
        }

        private void AddVisitTargetObjectAction(VisitTargetObjectAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _VisitTargetObject;
            _VisitTargetObject = (targetObject) => action(targetObject, previous);
        }

        private Action<object, bool> GetOutput()
        {
            // Redirect to file instead
            if (!string.IsNullOrEmpty(_Option.Output.Path))
            {
                var encoding = GetEncoding(_Option.Output.Encoding);
                return (object o, bool enumerate) => WriteToFile(
                    path: _Option.Output.Path,
                    shouldProcess: _ShouldProcess,
                    encoding: encoding,
                    o: o
                );
            }
            return _Output;
        }

        /// <summary>
        /// Get the character encoding for the specified output encoding.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static Encoding GetEncoding(OutputEncoding? encoding)
        {
            switch (encoding)
            {
                case OutputEncoding.UTF8:
                    return Encoding.UTF8;

                case OutputEncoding.UTF7:
                    return Encoding.UTF7;

                case OutputEncoding.Unicode:
                    return Encoding.Unicode;

                case OutputEncoding.UTF32:
                    return Encoding.UTF32;

                case OutputEncoding.ASCII:
                    return Encoding.ASCII;

                default:
                    return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            }
        }

        /// <summary>
        /// Write output to file.
        /// </summary>
        /// <param name="path">The file path to write.</param>
        /// <param name="encoding">The file encoding to use.</param>
        /// <param name="o">The text to write.</param>
        private static void WriteToFile(string path, ShouldProcess shouldProcess, Encoding encoding, object o)
        {
            var rootedPath = PSRuleOption.GetRootedPath(path: path);
            var parentPath = Directory.GetParent(rootedPath);
            if (!parentPath.Exists && shouldProcess(target: parentPath.FullName, action: PSRuleResources.ShouldCreatePath))
            {
                Directory.CreateDirectory(path: parentPath.FullName);
            }
            if (shouldProcess(target: rootedPath, action: PSRuleResources.ShouldWriteFile))
            {
                File.WriteAllText(path: rootedPath, contents: o.ToString(), encoding: encoding);
            }
        }
    }
}
