// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            var builder = PipelineBuilder.Invoke(GetSource(), option);
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        [Fact]
        public void InvokePipeline()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "FromFile1" };
            var builder = PipelineBuilder.Invoke(GetSource(), option);
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
            var builder = PipelineBuilder.Get(GetSource(), new PSRuleOption());
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
        }

        [Fact]
        public void PipelineWithInvariantCulture()
        {
            PSRuleOption.UseCurrentCulture(CultureInfo.InvariantCulture);
            var context = PipelineContext.New(GetOption(), null, null, new OptionContext(), null);
            var writer = new TestWriter(GetOption());
            var pipeline = new GetRulePipeline(context, GetSource(), new PipelineReader(null, null), writer, false);
            try
            {
                pipeline.Begin();
                pipeline.Process(null);
                pipeline.End();
                Assert.Contains(writer.Warnings, (string s) => { return s == PSRuleResources.UsingInvariantCulture; });
            }
            finally
            {
                PSRuleOption.UseCurrentCulture();
            }
        }

        private static Source[] GetSource()
        {
            var builder = new RuleSourceBuilder();
            builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
            return builder.Build();
        }

        private PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
