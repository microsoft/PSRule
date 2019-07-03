using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct an invoke pipeline.
    /// </summary>
    public sealed class InvokeRulePipelineBuilder
    {
        private readonly PSRuleOption _Option;
        private readonly PipelineLogger _Logger;

        private RuleSource[] _Source;
        private Hashtable _Tag;
        private RuleOutcome _Outcome;
        private BindTargetName _BindTargetNameHook;
        private BindTargetName _BindTargetTypeHook;
        private VisitTargetObject _VisitTargetObject;
        private ShouldProcess _ShouldProcess;
        private Action<object, bool> _Output;
        private bool _ReturnBoolean;
        private IPipelineStream _Stream;
        private string[] _InputPath;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
            _Outcome = RuleOutcome.Processed;
            _BindTargetNameHook = PipelineHookActions.DefaultTargetNameBinding;
            _BindTargetTypeHook = PipelineHookActions.DefaultTargetTypeBinding;
            _VisitTargetObject = PipelineReceiverActions.PassThru;
            _Output = (r, b) => { };
            _Stream = null;
            _InputPath = null;
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

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.UseCommandRuntime(commandRuntime);
            _ShouldProcess = commandRuntime.ShouldProcess;
            _Output = commandRuntime.WriteObject;
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _Logger.UseExecutionContext(executionContext);
        }

        public void AddBindTargetNameAction(BindTargetNameAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _BindTargetNameHook;
            _BindTargetNameHook = (targetObject) => action(targetObject, previous);
        }

        public void AddBindTargetTypeAction(BindTargetNameAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _BindTargetTypeHook;
            _BindTargetTypeHook = (targetObject) => action(targetObject, previous);
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
            _Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
            _Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
            _Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;

            _Option.Binding.IgnoreCase = option.Binding.IgnoreCase ?? BindingOption.Default.IgnoreCase;

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
                        return PipelineHookActions.NestedTargetNameBinding(
                            propertyNames: option.Binding.TargetName,
                            caseSensitive: !_Option.Binding.IgnoreCase.Value,
                            targetObject: targetObject,
                            next: next
                        );
                    });
                }
                else
                {
                    AddBindTargetNameAction((targetObject, next) =>
                    {
                        return PipelineHookActions.CustomTargetNameBinding(
                            propertyNames: option.Binding.TargetName,
                            caseSensitive: !_Option.Binding.IgnoreCase.Value,
                            targetObject: targetObject,
                            next: next
                        );
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

            if (option.Binding.TargetType != null && option.Binding.TargetType.Length > 0)
            {
                // Use nested TargetType binding when '.' is included in field name because it's slower then custom
                var useNested = option.Binding.TargetType.Any(n => n.Contains('.'));

                if (useNested)
                {
                    AddBindTargetTypeAction((targetObject, next) =>
                    {
                        return PipelineHookActions.NestedTargetNameBinding(
                            propertyNames: option.Binding.TargetType,
                            caseSensitive: !_Option.Binding.IgnoreCase.Value,
                            targetObject: targetObject,
                            next: next
                        );
                    });
                }
                else
                {
                    AddBindTargetTypeAction((targetObject, next) =>
                    {
                        return PipelineHookActions.CustomTargetNameBinding(
                            propertyNames: option.Binding.TargetType,
                            caseSensitive: !_Option.Binding.IgnoreCase.Value,
                            targetObject: targetObject,
                            next: next
                        );
                    });
                }
            }

            if (option.Pipeline.BindTargetType != null && option.Pipeline.BindTargetType.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (_Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                {
                    throw new PipelineConfigurationException(optionName: "BindTargetType", message: "Binding functions are not supported in this language mode.");
                }

                foreach (var action in option.Pipeline.BindTargetType)
                {
                    AddBindTargetTypeAction((targetObject, next) =>
                    {
                        var targetType = action(targetObject);

                        return string.IsNullOrEmpty(targetType) ? next(targetObject) : targetType;
                    });
                }
            }

            if (option.Suppression.Count > 0)
            {
                _Option.Suppression = new SuppressionOption(option.Suppression);
            }

            _Logger.Configure(_Option);
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

            // Redirect to file instead
            if (!string.IsNullOrEmpty(_Option.Output.Path))
            {
                var encoding = GetEncoding(_Option.Output.Encoding);

                _Output = (object o, bool enumerate) => WriteToFile(
                    path: _Option.Output.Path,
                    shouldProcess: _ShouldProcess,
                    encoding: encoding,
                    o: o
                );
            }

            if (_Stream == null)
            {
                _Stream = new PowerShellPipelineStream(option: _Option, output: _Output, returnBoolean: _ReturnBoolean, inputPath: _InputPath);
            }

            var filter = new RuleFilter(ruleName: _Option.Baseline.RuleName, tag: _Tag, exclude: _Option.Baseline.Exclude);
            var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: _BindTargetNameHook, bindTargetType: _BindTargetTypeHook);
            var pipeline = new InvokeRulePipeline(
                streamManager: new StreamManager(option: _Option, stream: _Stream, input: _VisitTargetObject),
                option: _Option,
                source: _Source,
                filter: filter,
                outcome: _Outcome,
                context: context
            );

            return pipeline;
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

            if (!parentPath.Exists)
            {
                if (shouldProcess(target: parentPath.FullName, action: PSRuleResources.ShouldCreatePath))
                {
                    Directory.CreateDirectory(path: parentPath.FullName);
                }
            }

            if (shouldProcess(target: rootedPath, action: PSRuleResources.ShouldWriteFile))
            {
                File.WriteAllText(path: rootedPath, contents: o.ToString(), encoding: encoding);
            }
        }
    }
}
