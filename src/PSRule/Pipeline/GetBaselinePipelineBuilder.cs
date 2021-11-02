// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;

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

        public override IPipeline Build(IPipelineWriter writer = null)
        {
            var filter = new BaselineFilter(_Name);
            return new GetBaselinePipeline(
                pipeline: PrepareContext(null, null, null),
                source: Source,
                reader: PrepareReader(),
                writer: writer ?? PrepareWriter(),
                filter: filter
            );
        }

        private static OutputFormat SuppressFormat(OutputFormat? format)
        {
            return !format.HasValue || format != OutputFormat.Yaml ? OutputFormat.None : format.Value;
        }
    }
}