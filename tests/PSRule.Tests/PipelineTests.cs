// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using System;
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
            var option = GetOption();
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            Assert.NotNull(builder.Build());
        }

        [Fact]
        public void InvokePipeline()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = GetOption();
            option.Rule.Include = new string[] { "FromFile1" };
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            var pipeline = builder.Build();
            pipeline.Begin();

            for (var i = 0; i < 100; i++)
            {
                pipeline.Process(PSObject.AsPSObject(testObject1));
            }
            pipeline.End();
        }

        [Fact]
        public void BuildGetPipeline()
        {
            var builder = PipelineBuilder.Get(GetSource(), GetOption(), null, null);
            Assert.NotNull(builder.Build());
        }

        [Fact]
        public void PipelineWithInvariantCulture()
        {
            PSRuleOption.UseCurrentCulture(CultureInfo.InvariantCulture);
            var context = PipelineContext.New(GetOption(), null, null, null, new OptionContext(), null);
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

        [Fact]
        public void PipelineWithOptions()
        {
            var option = GetOption(GetSourcePath("PSRule.Tests.yml"));
            var builder = PipelineBuilder.Get(GetSource(), option, null, null);
            Assert.NotNull(builder.Build());
        }

        [Fact]
        public void PipelineWithRequires()
        {
            var option = GetOption(GetSourcePath("PSRule.Tests6.yml"));
            var builder = PipelineBuilder.Get(GetSource(), option, null, null);
            Assert.Null(builder.Build());
        }

        #region Helper methods

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
            return builder.Build();
        }

        private PSRuleOption GetOption(string path = null)
        {
            if (path == null)
                return new PSRuleOption();

            return PSRuleOption.FromFile(path);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
