// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;

namespace PSRule.Pipeline
{
    internal sealed class ExportBaselinePipelineBuilder : PipelineBuilderBase
    {
        private string[] _Name;

        internal ExportBaselinePipelineBuilder(Source[] source, HostContext hostContext)
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
            Option.Output.Format = option.Output.Format;
            Option.Output.Encoding = option.Output.Encoding;
            Option.Output.Path = option.Output.Path;

            return this;
        }

        public override IPipeline Build()
        {
            BaselineFilter filter = new BaselineFilter(_Name);
            return new GetBaselinePipeline(
                pipeline: PrepareContext(null, null, null),
                source: Source,
                reader: PrepareReader(),
                writer: PrepareWriter(),
                filter: filter
            );
        }
    }
}