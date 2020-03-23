// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline.Output;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static IInvokePipelineBuilder Assert(Source[] source, PSRuleOption option)
        {
            var pipeline = new AssertPipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IInvokePipelineBuilder Invoke(Source[] source, PSRuleOption option)
        {
            var pipeline = new InvokeRulePipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IInvokePipelineBuilder Test(Source[] source, PSRuleOption option)
        {
            var pipeline = new TestPipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IGetPipelineBuilder Get(Source[] source, PSRuleOption option)
        {
            var pipeline = new GetRulePipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IHelpPipelineBuilder GetHelp(Source[] source, PSRuleOption option)
        {
            var pipeline = new GetRuleHelpPipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }

        public static RuleSourceBuilder RuleSource()
        {
            return new RuleSourceBuilder();
        }

        public static IPipelineBuilder GetBaseline(Source[] source, PSRuleOption option)
        {
            var pipeline = new GetBaselinePipelineBuilder(source);
            pipeline.Configure(option);
            return pipeline;
        }
    }

    public interface IPipelineBuilder
    {
        void UseCommandRuntime(PSCmdlet commandRuntime);

        void UseExecutionContext(EngineIntrinsics executionContext);

        IPipelineBuilder Configure(PSRuleOption option);

        IPipeline Build();
    }

    public interface IPipeline
    {
        void Begin();

        void Process(PSObject sourceObject);

        void End();
    }

    internal abstract class PipelineBuilderBase : IPipelineBuilder
    {
        protected readonly PSRuleOption Option;
        protected readonly Source[] Source;
        protected readonly HostContext HostContext;
        protected PSCmdlet CmdletContext;
        protected EngineIntrinsics ExecutionContext;

        private string[] _Include;
        private Hashtable _Tag;
        private BaselineOption _Baseline;

        private ShouldProcess _ShouldProcess;
        private readonly PSPipelineWriter _Output;

        protected PipelineBuilderBase(Source[] source)
        {
            Option = new PSRuleOption();
            Source = source;
            _Output = new PSPipelineWriter(Option);
            HostContext = new HostContext();
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

        public virtual void UseCommandRuntime(PSCmdlet commandRuntime)
        {
            CmdletContext = commandRuntime;
            _ShouldProcess = commandRuntime.ShouldProcess;
            _Output.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            ExecutionContext = executionContext;
            HostContext.InSession = executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
            _Output.UseExecutionContext(executionContext);
        }

        public virtual IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Binding = new BindingOption(option.Binding);
            Option.Execution = new ExecutionOption(option.Execution);
            Option.Input = new InputOption(option.Input);
            Option.Input.Format = Option.Input.Format ?? InputOption.Default.Format;
            Option.Output = new OutputOption(option.Output);
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

        protected PipelineContext PrepareContext(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField)
        {
            var unresolved = new Dictionary<string, ResourceRef>(StringComparer.OrdinalIgnoreCase);
            if (_Baseline is BaselineOption.BaselineRef baselineRef)
                unresolved.Add(baselineRef.Name, new BaselineRef(baselineRef.Name, BaselineContext.ScopeType.Explicit));

            for (var i = 0; i < Source.Length; i++)
            {
                if (Source[i].Module != null && Source[i].Module.Baseline != null && !unresolved.ContainsKey(Source[i].Module.Baseline))
                    unresolved.Add(Source[i].Module.Baseline, new BaselineRef(Source[i].Module.Baseline, BaselineContext.ScopeType.Module));
            }

            return PipelineContext.New(
                option: Option,
                hostContext: HostContext,
                binder: new TargetBinder(bindTargetName, bindTargetType, bindField, Option.Input.TargetType),
                baseline: GetBaselineContext(),
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
                    shouldProcess: _ShouldProcess
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

            // Fallback to current culture
            var current = PSRuleOption.GetCurrentCulture();
            if (!set.Contains(current.Name))
            {
                result.Add(current.Name);
                set.Add(current.Name);
            }
            for (var p = current.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
            {
                if (!result.Contains(p.Name))
                    result.Add(p.Name);
            }
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

        private BaselineContext GetBaselineContext()
        {
            var result = new BaselineContext();
            var scope = new BaselineContext.BaselineContextScope(type: BaselineContext.ScopeType.Workspace, moduleName: null, option: Option);
            result.Add(scope);

            scope = new BaselineContext.BaselineContextScope(type: BaselineContext.ScopeType.Parameter, include: _Include, tag: _Tag);
            result.Add(scope);

            return result;
        }
    }
}
