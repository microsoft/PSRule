using PSRule.Pipeline;
using System;
using System.IO;
using Xunit;

namespace PSRule
{
    public sealed class PipelineTests
    {
        [Fact]
        public void BuildInvokePipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(GetTestSource());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        [Fact]
        public void BuildGetPipeline()
        {
            var builder = PipelineBuilder.Get();
            builder.Source(GetTestSource());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        private static string[] GetTestSource()
        {
            return new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FromFile.Rule.ps1") };
        }
    }
}
