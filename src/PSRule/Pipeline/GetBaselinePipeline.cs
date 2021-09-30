// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Host;
using System.Linq;

namespace PSRule.Pipeline
{
    internal sealed class GetBaselinePipelineBuilder : PipelineBuilderBase
    {
        private string[] _Name;

        internal GetBaselinePipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }

        /// <summary>
        /// Filter returned baselines by name.
        /// </summary>
        public new void Name(string[] name)
        {
            if (name == null || name.Length == 0)
                return;

            _Name = name;
        }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Output.As = ResultFormat.Detail;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Format = SuppressFormat(option.Output.Format);
            return this;
        }

        public override IPipeline Build()
        {
            var filter = new BaselineFilter(_Name);
            return new GetBaselinePipeline(
                pipeline: PrepareContext(null, null, null),
                source: Source,
                reader: PrepareReader(),
                writer: PrepareWriter(),
                filter: filter
            );
        }

        private static OutputFormat SuppressFormat(OutputFormat? format)
        {
            return !format.HasValue || format != OutputFormat.Yaml ? OutputFormat.None : format.Value;
        }
    }

    internal sealed class GetBaselinePipeline : RulePipeline
    {
        private readonly IResourceFilter _Filter;

        internal GetBaselinePipeline(PipelineContext pipeline, Source[] source, PipelineReader reader, PipelineWriter writer, IResourceFilter filter)
            : base(pipeline, source, reader, writer)
        {
            _Filter = filter;
        }

        public override void End()
        {
            Writer.WriteObject(HostHelper.GetBaselineYaml(Source, Context).Where(Match), true);
            Writer.End();
        }

        private bool Match(Baseline baseline)
        {
            return _Filter == null || _Filter.Match(baseline);
        }
    }
}
