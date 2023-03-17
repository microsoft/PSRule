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
            var builder = PipelineBuilder.Invoke(GetSource(), option, null);
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
            var builder = PipelineBuilder.Invoke(GetSource(), option, null);
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
            builder = PipelineBuilder.Invoke(GetSource(), option, null);
            pipeline = builder.Build(writer);
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
            actual1 = writer.Output[0] as InvokeResult;
            actual2 = actual1.AsRecord()[0].Data["count"];
            Assert.Equal(11, actual2);
        }

        /// <summary>
        /// Test that localized data is accessible to conventions at each block.
        /// </summary>
        [Fact]
        public void WithLocalizedData()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = GetOption();
            option.Rule.Include = new string[] { "WithLocalizedDataPrecondition" };
            option.Convention.Include = new string[] { "Convention.WithLocalizedData" };
            var writer = new TestWriter(option);
            var builder = PipelineBuilder.Invoke(GetSource(), option, null);
            var pipeline = builder.Build(writer);

            Assert.NotNull(pipeline);
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
            Assert.NotEmpty(writer.Information);
            Assert.Equal("LocalizedMessage for en. Format=Initialize.", writer.Information[0] as string);
            Assert.Equal("LocalizedMessage for en. Format=Begin.", writer.Information[1] as string);
            Assert.Equal("LocalizedMessage for en. Format=Precondition.", writer.Information[2] as string);
            Assert.Equal("LocalizedMessage for en. Format=Process.", writer.Information[3] as string);
            Assert.Equal("LocalizedMessage for en. Format=End.", writer.Information[4] as string);
        }

        #region Helper methods

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFileConventions.Rule.ps1"));
            return builder.Build();
        }

        private static PSRuleOption GetOption(string path = null)
        {
            var option = path == null ? new PSRuleOption() : PSRuleOption.FromFile(path);
            option.Output.Culture = new[] { "en-US" };
            return option;
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
