// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule.Pipeline
{
    internal sealed class TestPipelineBuilder : InvokePipelineBuilderBase
    {
        internal TestPipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }

        private sealed class BooleanWriter : PipelineWriter
        {
            private readonly RuleOutcome _Outcome;

            internal BooleanWriter(PipelineWriter output, RuleOutcome outcome)
                : base(output, null)
            {
                _Outcome = outcome;
            }

            public override void WriteObject(object sendToPipeline, bool enumerateCollection)
            {
                if (!(sendToPipeline is InvokeResult result) || !ShouldOutput(result.Outcome))
                    return;

                base.WriteObject(result.IsSuccess(), false);
            }

            private bool ShouldOutput(RuleOutcome outcome)
            {
                return _Outcome == RuleOutcome.All ||
                    (_Outcome == RuleOutcome.None && outcome == RuleOutcome.None) ||
                    (_Outcome & outcome) > 0;
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return new BooleanWriter(GetOutput(), Option.Output.Outcome.Value);
        }
    }
}
