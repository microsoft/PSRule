// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Pipeline;
using Xunit;
using static PSRule.PipelineTests;

namespace PSRule
{
    public sealed class ConventionTests
    {
        [Fact]
        public void WithConventions()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = GetOption();
            option.Rule.Include = new string[] { "ConventionTest" };
            option.Convention.Include = new string[] { "Convention1" };
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            var pipeline = builder.Build();

            Assert.NotNull(pipeline);
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
        }

        [Fact]
        public void ConventionOrder()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = GetOption();
            option.Rule.Include = new string[] { "ConventionTest" };

            // Order 1
            option.Convention.Include = new string[] { "Convention1", "Convention2" };
            var writer = new TestWriter(option);
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            var pipeline = builder.Build(writer);
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
            var actual1 = writer.Output[0] as InvokeResult;
            var actual2 = actual1.AsRecord()[0].Data["count"];
            Assert.Equal(110, actual2);

            // Order 2
            option.Convention.Include = new string[] { "Convention2", "Convention1" };
            writer = new TestWriter(option);
            builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            pipeline = builder.Build(writer);
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
            actual1 = writer.Output[0] as InvokeResult;
            actual2 = actual1.AsRecord()[0].Data["count"];
            Assert.Equal(11, actual2);
        }

        #region Helper methods

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFileConventions.Rule.ps1"));
            return builder.Build();
        }

        private PSRuleOption GetOption(string path = null)
        {
            return path == null ? new PSRuleOption() : PSRuleOption.FromFile(path);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
