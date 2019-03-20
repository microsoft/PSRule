using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Rules;
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
            var option = new PSRuleOption();
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(GetTestSource());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        [Fact]
        public void InvokePipeline()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "FromFile1" };
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(GetTestSource());
            var pipeline = builder.Build();
            pipeline.Begin();

            var actual = new List<InvokeResult>();

            for (var i = 0; i < 100; i++)
            {
                pipeline.Process(PSObject.AsPSObject(testObject1));
            }

            pipeline.End();
        }

        [Fact]
        public void BuildGetPipeline()
        {
            var builder = PipelineBuilder.Get();
            builder.Source(GetTestSource());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        private static RuleSource[] GetTestSource()
        {
            return new RuleSource[] { new RuleSource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FromFile.Rule.ps1"), null) };
        }
    }
}
