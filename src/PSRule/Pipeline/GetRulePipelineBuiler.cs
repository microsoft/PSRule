// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

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

        internal GetRulePipelineBuilder(Source[] source, IHostContext hostContext)
            : base(source, hostContext) { }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Execution.InvariantCultureWarning = option.Execution.InvariantCultureWarning ?? ExecutionOption.Default.InvariantCultureWarning;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Format = SuppressFormat(option.Output.Format);
            Option.Requires = new RequiresOption(option.Requires);
            Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            return this;
        }
        public void IncludeDependencies()
        {
            _IncludeDependencies = true;
        }

        public override IPipeline Build(IPipelineWriter writer = null)
        {
            return !RequireModules() || !RequireSources()
                ? null
                : (IPipeline)new GetRulePipeline(
                    pipeline: PrepareContext(
                        bindTargetName: null,
                        bindTargetType: null,
                        bindField: null
                    ),
                    source: Source,
                    reader: PrepareReader(),
                    writer: writer ?? PrepareWriter(),
                    includeDependencies: _IncludeDependencies
                );
        }

        private static OutputFormat SuppressFormat(OutputFormat? format)
        {
            return !format.HasValue ||
                !(format == OutputFormat.Wide ||
                format == OutputFormat.Json ||
                format == OutputFormat.Yaml) ? OutputFormat.None : format.Value;
        }
    }
}