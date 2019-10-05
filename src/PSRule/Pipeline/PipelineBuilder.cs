using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

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

        public static IPipelineBuilder Get(Source[] source, PSRuleOption option)
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
        void UseCommandRuntime(ICommandRuntime2 commandRuntime);

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
        protected readonly PipelineLogger Logger;
        protected readonly PSRuleOption Option;
        protected readonly Source[] Source;
        protected readonly HostContext HostContext;

        private string[] _Include;
        private Hashtable _Tag;
        private BaselineOption _Baseline;

        private ShouldProcess _ShouldProcess;
        private WriteOutput _Output;

        protected PipelineBuilderBase(Source[] source)
        {
            Logger = new PipelineLogger();
            Option = new PSRuleOption();
            Source = source;
            _Output = (r, b) => { };
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

        public virtual void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            Logger.UseCommandRuntime(commandRuntime);
            _ShouldProcess = commandRuntime.ShouldProcess;
            _Output = commandRuntime.WriteObject;
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            HostContext.InSession = executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
            Logger.UseExecutionContext(executionContext);
        }

        public virtual IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Binding = new BindingOption(option.Binding);
            Option.Execution = new ExecutionOption(option.Execution);
            Option.Output = new OutputOption(option.Output);
            return this;
        }

        public abstract IPipeline Build();

        protected void ConfigureLogger(PSRuleOption option)
        {
            Logger.Configure(option);
        }

        /// <summary>
        /// Use a baseline, either by name or by path.
        /// </summary>
        public void UseBaseline(BaselineOption baseline)
        {
            if (baseline == null)
                return;

            _Baseline = baseline;
        }

        protected PipelineContext PrepareContext(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType)
        {
            var unresolved = new Dictionary<string, ResourceRef>(StringComparer.OrdinalIgnoreCase);
            if (_Baseline != null && _Baseline is BaselineOption.BaselineRef baselineRef)
                unresolved.Add(baselineRef.Name, new BaselineRef(baselineRef.Name, BaselineContext.ScopeType.Explicit));

            for (var i = 0; i < Source.Length; i++)
            {
                if (Source[i].Module != null && Source[i].Module.Baseline != null)
                    unresolved.Add(Source[i].Module.Baseline, new BaselineRef(Source[i].Module.Baseline, BaselineContext.ScopeType.Module));
            }

            return PipelineContext.New(
                logger: Logger,
                option: Option,
                hostContext: HostContext,
                binder: new TargetBinder(bindTargetName: bindTargetName, bindTargetType: bindTargetType),
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
                    return new CSVSerializer(output);

                case OutputFormat.Json:
                    return new JsonOutputWriter(output);

                case OutputFormat.NUnit3:
                    return new NUnit3Serializer(output);

                case OutputFormat.Yaml:
                    return new YamlOutputWriter(output);

                default:
                    return new PassThruWriter(output, Option.Output.Format == OutputFormat.Wide);
            }
        }

        protected WriteOutput GetOutput()
        {
            // Redirect to file instead
            if (!string.IsNullOrEmpty(Option.Output.Path))
            {
                var encoding = GetEncoding(Option.Output.Encoding);
                return (object o, bool enumerate) => WriteToFile(
                    path: Option.Output.Path,
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
