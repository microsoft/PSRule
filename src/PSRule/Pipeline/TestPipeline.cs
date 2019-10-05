using PSRule.Rules;

namespace PSRule.Pipeline
{
    internal sealed class TestPipelineBuilder : InvokePipelineBuilderBase
    {
        internal TestPipelineBuilder(Source[] source)
            : base(source) { }

        private sealed class BooleanWriter : PipelineWriter
        {
            internal BooleanWriter(WriteOutput output)
                : base(output) { }

            public override void Write(object o, bool enumerate)
            {
                if (!(o is InvokeResult result))
                    return;

                base.Write(result.IsSuccess(), false);
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return new BooleanWriter(GetOutput());
        }
    }
}
