// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule.Pipeline
{
    internal sealed class TestPipelineBuilder : InvokePipelineBuilderBase
    {
        internal TestPipelineBuilder(Source[] source)
            : base(source) { }

        private sealed class BooleanWriter : PipelineWriter
        {
            private readonly RuleOutcome _Outcome;

            internal BooleanWriter(WriteOutput output, RuleOutcome outcome)
                : base(output)
            {
                _Outcome = outcome;
            }

            public override void Write(object o, bool enumerate)
            {
                if (!(o is InvokeResult result) || !ShouldOutput(result.Outcome))
                    return;

                base.Write(result.IsSuccess(), false);
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
            return new BooleanWriter(GetOutput(), Outcome);
        }
    }
}
