using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Threading;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    internal sealed class GetRulePipelineBuilder : PipelineBuilderBase
    {
        internal GetRulePipelineBuilder(Source[] source)
            : base(source) { }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Output.Culture = option.Output.Culture ?? new string[] { Thread.CurrentThread.CurrentCulture.ToString() };
            Option.Output.Format = SuppressFormat(option.Output.Format);

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            ConfigureLogger(Option);
            return this;
        }

        public override IPipeline Build()
        {
            return new GetRulePipeline(PrepareContext(null, null), Source, PrepareReader(), PrepareWriter());
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
        internal GetRulePipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer)
            : base(context, source, reader, writer)
        {
            // Do nothing
        }

        public override void End()
        {
            Writer.Write(HostHelper.GetRule(source: Source, context: Context), true);
            Writer.End();
        }
    }
}
