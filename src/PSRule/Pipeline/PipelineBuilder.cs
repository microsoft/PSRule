// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline.Output;
using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static IInvokePipelineBuilder Assert(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new AssertPipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IInvokePipelineBuilder Invoke(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IInvokePipelineBuilder Test(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new TestPipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IGetPipelineBuilder Get(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new GetRulePipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IHelpPipelineBuilder GetHelp(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new GetRuleHelpPipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static SourcePipelineBuilder RuleSource(PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new SourcePipelineBuilder(hostContext, option);
            return pipeline;
        }

        public static IPipelineBuilder GetBaseline(Source[] source, PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new GetBaselinePipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IGetTargetPipelineBuilder GetTarget(PSRuleOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var pipeline = new GetTargetPipelineBuilder(null, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }
    }

    public interface IPipelineBuilder
    {
        IPipelineBuilder Configure(PSRuleOption option);

        IPipeline Build();
    }

    public interface IPipeline
    {
        void Begin();

        void Process(PSObject sourceObject);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches PowerShell pipeline.")]
        void End();
    }

    internal abstract class PipelineBuilderBase : IPipelineBuilder
    {
        private const string ENGINE_MODULE_NAME = "PSRule";

        protected readonly PSRuleOption Option;
        protected readonly Source[] Source;
        protected readonly HostContext HostContext;
        protected BindTargetMethod BindTargetNameHook;
        protected BindTargetMethod BindTargetTypeHook;
        protected BindTargetMethod BindFieldHook;
        protected VisitTargetObject VisitTargetObject;

        private string[] _Include;
        private Hashtable _Tag;
        private BaselineOption _Baseline;

        private readonly HostPipelineWriter _Output;

        protected PipelineBuilderBase(Source[] source, HostContext hostContext)
        {
            Option = new PSRuleOption();
            Source = source;
            _Output = new HostPipelineWriter(hostContext, Option);
            HostContext = hostContext;
            BindTargetNameHook = PipelineHookActions.BindTargetName;
            BindTargetTypeHook = PipelineHookActions.BindTargetType;
            BindFieldHook = PipelineHookActions.BindField;
            VisitTargetObject = PipelineReceiverActions.PassThru;
        }

        public void Name(string[] name)
        {
            if (name == null || name.Length == 0)
                return;

            _Include = name;
        }

        public void Tag(Hashtable tag)
        {
            if (tag == null || tag.Count == 0)
                return;

            _Tag = tag;
        }

        public virtual IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Binding = new BindingOption(option.Binding);
            Option.Execution = new ExecutionOption(option.Execution);
            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Input = new InputOption(option.Input);
            Option.Input.Format = Option.Input.Format ?? InputOption.Default.Format;
            Option.Output = new OutputOption(option.Output);
            Option.Output.Outcome = Option.Output.Outcome ?? OutputOption.Default.Outcome;
            return this;
        }

        public abstract IPipeline Build();

        /// <summary>
        /// Use a baseline, either by name or by path.
        /// </summary>
        public void UseBaseline(BaselineOption baseline)
        {
            if (baseline == null)
                return;

            _Baseline = baseline;
        }

        /// <summary>
        /// Require correct module versions for pipeline execution.
        /// </summary>
        protected bool RequireModules()
        {
            var result = true;
            if (Option.Requires.TryGetValue(ENGINE_MODULE_NAME, out string requiredVersion))
            {
                var engineVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                if (GuardModuleVersion(ENGINE_MODULE_NAME, engineVersion, requiredVersion))
                    result = false;
            }
            for (var i = 0; Source != null && i < Source.Length; i++)
            {
                if (Source[i].Module != null && Option.Requires.TryGetValue(Source[i].Module.Name, out requiredVersion))
                {
                    if (GuardModuleVersion(Source[i].Module.Name, Source[i].Module.Version, requiredVersion))
                        result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Require sources for pipeline execution.
        /// </summary>
        protected bool RequireSources()
        {
            if (Source == null || Source.Length == 0)
            {
                PrepareWriter().WarnRulePathNotFound();
                return false;
            }
            return true;
        }

        private bool GuardModuleVersion(string moduleName, string moduleVersion, string requiredVersion)
        {
            if (!TryModuleVersion(moduleVersion, requiredVersion))
            {
                var writer = PrepareWriter();
                writer.ErrorRequiredVersionMismatch(moduleName, moduleVersion, requiredVersion);
                writer.End();
                return true;
            }
            return false;
        }

        private static bool TryModuleVersion(string moduleVersion, string requiredVersion)
        {
            if (!(SemanticVersion.TryParseVersion(moduleVersion, out SemanticVersion.Version version) && SemanticVersion.TryParseConstraint(requiredVersion, out SemanticVersion.Constraint constraint)))
                return false;

            return constraint.Equals(version);
        }

        protected PipelineContext PrepareContext(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField)
        {
            var unresolved = new Dictionary<string, ResourceRef>(StringComparer.OrdinalIgnoreCase);
            if (_Baseline is BaselineOption.BaselineRef baselineRef)
                unresolved.Add(baselineRef.Name, new BaselineRef(baselineRef.Name, OptionContext.ScopeType.Explicit));

            for (var i = 0; Source != null && i < Source.Length; i++)
            {
                if (Source[i].Module != null && Source[i].Module.Baseline != null && !unresolved.ContainsKey(Source[i].Module.Baseline))
                    unresolved.Add(Source[i].Module.Baseline, new BaselineRef(Source[i].Module.Baseline, OptionContext.ScopeType.Module));
            }

            return PipelineContext.New(
                option: Option,
                hostContext: HostContext,
                binder: new TargetBinder(bindTargetName, bindTargetType, bindField, Option.Input.TargetType),
                baseline: GetOptionContext(),
                unresolved: unresolved
            );
        }

        protected virtual PipelineReader PrepareReader()
        {
            return new PipelineReader(null, null);
        }

        protected virtual PipelineWriter PrepareWriter()
        {
            var output = GetOutput();
            switch (Option.Output.Format)
            {
                case OutputFormat.Csv:
                    return new CsvOutputWriter(output, Option);

                case OutputFormat.Json:
                    return new JsonOutputWriter(output, Option);

                case OutputFormat.NUnit3:
                    return new NUnit3OutputWriter(output, Option);

                case OutputFormat.Yaml:
                    return new YamlOutputWriter(output, Option);

                case OutputFormat.Markdown:
                    return new MarkdownOutputWriter(output, Option);

                case OutputFormat.Wide:
                    return new WideOutputWriter(output, Option);
            }
            return output;
        }

        protected PipelineWriter GetOutput()
        {
            // Redirect to file instead
            if (!string.IsNullOrEmpty(Option.Output.Path))
            {
                return new FileOutputWriter(
                    inner: _Output,
                    option: Option,
                    encoding: GetEncoding(Option.Output.Encoding),
                    path: Option.Output.Path,
                    shouldProcess: HostContext.ShouldProcess
                );
            }
            return _Output;
        }

        protected static string[] GetCulture(string[] culture)
        {
            var result = new List<string>();
            var parent = new List<string>();
            var set = new HashSet<string>();
            for (var i = 0; culture != null && i < culture.Length; i++)
            {
                var c = CultureInfo.CreateSpecificCulture(culture[i]);
                if (!set.Contains(c.Name))
                {
                    result.Add(c.Name);
                    set.Add(c.Name);
                }
                for (var p = c.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
                {
                    if (!set.Contains(p.Name))
                    {
                        parent.Add(p.Name);
                        set.Add(p.Name);
                    }
                }
            }
            if (parent.Count > 0)
                result.AddRange(parent);

            if (result.Count == 0)
                return null;

            return result.ToArray();
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

        private OptionContext GetOptionContext()
        {
            var result = new OptionContext();

            // Baseline
            var baselineScope = new OptionContext.BaselineScope(type: OptionContext.ScopeType.Workspace, baselineId: null, moduleName: null, option: Option, obsolete: false);
            result.Add(baselineScope);
            baselineScope = new OptionContext.BaselineScope(type: OptionContext.ScopeType.Parameter, include: _Include, tag: _Tag);
            result.Add(baselineScope);

            // Config
            var configScope = new OptionContext.ConfigScope(type: OptionContext.ScopeType.Workspace, moduleName: null, option: Option);
            result.Add(configScope);

            return result;
        }

        protected void ConfigureBinding(PSRuleOption option)
        {
            if (option.Pipeline.BindTargetName != null && option.Pipeline.BindTargetName.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                    throw new PipelineConfigurationException(optionName: "BindTargetName", message: PSRuleResources.ConstrainedTargetBinding);

                foreach (var action in option.Pipeline.BindTargetName)
                    BindTargetNameHook = AddBindTargetAction(action, BindTargetNameHook);
            }

            if (option.Pipeline.BindTargetType != null && option.Pipeline.BindTargetType.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                    throw new PipelineConfigurationException(optionName: "BindTargetType", message: PSRuleResources.ConstrainedTargetBinding);

                foreach (var action in option.Pipeline.BindTargetType)
                    BindTargetTypeHook = AddBindTargetAction(action, BindTargetTypeHook);
            }
        }

        private static BindTargetMethod AddBindTargetAction(BindTargetFunc action, BindTargetMethod previous)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            return (propertyNames, caseSensitive, targetObject) => action(propertyNames, caseSensitive, targetObject, previous);
        }

        private static BindTargetMethod AddBindTargetAction(BindTargetName action, BindTargetMethod previous)
        {
            return AddBindTargetAction((parameterNames, caseSensitive, targetObject, next) =>
            {
                var targetType = action(targetObject);
                return string.IsNullOrEmpty(targetType) ? next(parameterNames, caseSensitive, targetObject) : targetType;
            }, previous);
        }

        protected void AddVisitTargetObjectAction(VisitTargetObjectAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = VisitTargetObject;
            VisitTargetObject = (targetObject) => action(targetObject, previous);
        }
    }
}
