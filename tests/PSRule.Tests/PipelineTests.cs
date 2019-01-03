using PSRule.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    public sealed class PipelineTests
    {
        internal class TestObject
        {
            public string Name { get; set; }
        }

        [Fact]
        public void BuildInvokePipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(GetTestSource());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        [Fact]
        public void InvokePipeline()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };

            var builder = PipelineBuilder.Invoke();
            builder.Source(GetTestSource());
            builder.FilterBy(new string[] { "FromFile1" }, null);
            var pipeline = builder.Build();

            var actual = new List<InvokeResult>();

            for (var i = 0; i < 100; i++)
            {
                actual.Add(pipeline.Process(PSObject.AsPSObject(testObject1)));
            }
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
