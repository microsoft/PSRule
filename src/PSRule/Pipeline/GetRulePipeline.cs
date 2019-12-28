// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public interface IGetPipelineBuilder : IPipelineBuilder
    {
        void IncludeDependencies();
    }

    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    internal sealed class GetRulePipelineBuilder : PipelineBuilderBase, IGetPipelineBuilder
    {
        private bool _IncludeDependencies;

        internal GetRulePipelineBuilder(Source[] source)
            : base(source) { }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Format = SuppressFormat(option.Output.Format);

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            ConfigureLogger(Option);
            return this;
        }
        public void IncludeDependencies()
        {
            _IncludeDependencies = true;
        }

        public override IPipeline Build()
        {
            return new GetRulePipeline(PrepareContext(null, null, null), Source, PrepareReader(), PrepareWriter(), _IncludeDependencies);
        }

        private OutputFormat SuppressFormat(OutputFormat? format)
        {
            return !format.HasValue ||
                !(Option.Output.Format == OutputFormat.Wide ||
                Option.Output.Format == OutputFormat.Json ||
                Option.Output.Format == OutputFormat.Yaml) ? OutputFormat.None : format.Value;
        }
    }

    internal sealed class GetRulePipeline : RulePipeline, IPipeline
    {
        private readonly bool _IncludeDependencies;

        internal GetRulePipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer, bool includeDependencies)
            : base(context, source, reader, writer)
        {
            HostHelper.ImportResource(source: Source, context: context);
            _IncludeDependencies = includeDependencies;
        }

        public override void End()
        {
            Writer.Write(HostHelper.GetRule(Source, Context, _IncludeDependencies), true);
            Writer.End();
        }
    }
}
