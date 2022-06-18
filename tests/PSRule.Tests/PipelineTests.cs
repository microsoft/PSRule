// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
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

            Assert.NotNull(pipeline);
            pipeline.Begin();
            for (var i = 0; i < 100; i++)
                pipeline.Process(PSObject.AsPSObject(testObject1));

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
            var context = PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null);
            var writer = new TestWriter(GetOption());
            var pipeline = new GetRulePipeline(context, GetSource(), new PipelineReader(null, null, null), writer, false);
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
        public void PipelineWithInvariantCultureDisabled()
        {
            PSRuleOption.UseCurrentCulture(CultureInfo.InvariantCulture);
            var option = new PSRuleOption();
            option.Execution.InvariantCultureWarning = false;
            var context = PipelineContext.New(option, null, null, null, null, null, new OptionContext(), null);
            var writer = new TestWriter(option);
            var pipeline = new GetRulePipeline(context, GetSource(), new PipelineReader(null, null, null), writer, false);
            try
            {
                pipeline.Begin();
                pipeline.Process(null);
                pipeline.End();
                Assert.DoesNotContain(writer.Warnings, (string s) => { return s == PSRuleResources.UsingInvariantCulture; });
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

        /// <summary>
        /// An Invoke pipeline reading from an input file.
        /// </summary>
        [Fact]
        public void PipelineWithSource()
        {
            var option = GetOption();
            option.Rule.Include = new string[] { "FromFile1" };
            option.Input.PathIgnore = new string[]
            {
                "**/ObjectFromFile*.json",
                "!**/ObjectFromFile.json"
            };

            // Default
            var writer = new TestWriter(option);
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            builder.InputPath(new string[] { "./**/ObjectFromFile*.json" });
            var pipeline = builder.Build(writer);
            Assert.NotNull(pipeline);
            pipeline.Begin();
            pipeline.Process(GetTestObject());
            pipeline.Process(GetFileObject());
            pipeline.End();

            var items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
            Assert.Equal(4, items.Length);
            Assert.True(items[0].HasSource());
            Assert.True(items[1].HasSource());
            Assert.True(items[2].HasSource());
            Assert.True(items[3].HasSource());

            // With IgnoreObjectSource
            option.Input.IgnoreObjectSource = true;
            writer = new TestWriter(option);
            builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            PipelineBuilder.Invoke(GetSource(), option, null, null);
            builder.InputPath(new string[] { "./**/ObjectFromFile*.json" });
            pipeline = builder.Build(writer);
            Assert.NotNull(pipeline);
            pipeline.Begin();
            pipeline.Process(GetTestObject());
            pipeline.Process(GetFileObject());
            pipeline.End();

            items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
            Assert.Equal(2, items.Length);
            Assert.True(items[0].HasSource());
            Assert.True(items[1].HasSource());
        }

        /// <summary>
        /// An Invoke pipeline reading from an input file with File format.
        /// </summary>
        [Fact]
        public void PipelineWithFileFormat()
        {
            var option = GetOption();
            option.Input.Format = InputFormat.File;
            option.Rule.Include = new string[] { "FromFile1" };
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            builder.InputPath(new string[] { "./**/ObjectFromFile.json" });
            var writer = new TestWriter(option);
            var pipeline = builder.Build(writer);
            Assert.NotNull(pipeline);
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();

            var items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
            Assert.Single(items);
            Assert.True(items[0].HasSource());
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
            return path == null ? new PSRuleOption() : PSRuleOption.FromFile(path);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private static PSObject GetTestObject()
        {
            var info = new PSObject();
            var source = new PSObject();
            source.Properties.Add(new PSNoteProperty("file", PSRuleOption.GetRootedPath("./ObjectFromFileNotFile.json")));
            source.Properties.Add(new PSNoteProperty("type", "example"));
            info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
            var o = new PSObject();
            o.Properties.Add(new PSNoteProperty("name", "FromObjectTest"));
            o.Properties.Add(new PSNoteProperty("_PSRule", info));
            return o;
        }

        private static PSObject GetFileObject()
        {
            var info = new FileInfo(PSRuleOption.GetRootedPath("./ObjectFromFileSingle.json"));
            return new PSObject(info);
        }

        #endregion Helper methods
    }
}
