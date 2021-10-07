﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSRule.Pipeline;
using Xunit;

namespace PSRule
{
    public sealed class InputFormatDeserializerTests
    {
        [Fact]
        public void DeserializeObjectsYaml()
        {
            var actual = PipelineReceiverActions.ConvertFromYaml(GetYamlContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
            Assert.Equal("Test", actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<string>("kind"));
            Assert.Equal(2, actual[1].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<int>("value2"));
            Assert.Equal(2, actual[1].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<PSObject[]>("array").Length);
            Assert.Equal("TestObject1", PipelineHookActions.BindTargetName(null, false, false, actual[0].Value));

            // Array item
            actual = PipelineReceiverActions.ConvertFromYaml(GetYamlContent("3"), PipelineReceiverActions.PassThru).ToArray();
            Assert.Equal(2, actual.Length);
            Assert.Equal("item1", actual[0].Value.PropertyValue<string>("name"));
            Assert.Equal("value1", actual[0].Value.PropertyValue<string>("value"));
            Assert.Equal("item2", actual[1].Value.PropertyValue<string>("name"));
            Assert.Equal("value2", actual[1].Value.PropertyValue<string>("value"));
        }

        [Fact]
        public void DeserializeObjectsJson()
        {
            var actual = PipelineReceiverActions.ConvertFromJson(GetJsonContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
            Assert.Equal("Test", actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<string>("kind"));
            Assert.Equal(2, actual[1].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<int>("value2"));
            Assert.Equal(3, actual[1].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<PSObject[]>("array").Length);
            Assert.Equal("TestObject1", PipelineHookActions.BindTargetName(null, false, false, actual[0].Value));
            actual[0].Value.TryTargetInfo(out Runtime.PSRuleTargetInfo info1);
            actual[1].Value.TryTargetInfo(out Runtime.PSRuleTargetInfo info2);
            Assert.Equal("some-file.json", info1.Source[0].File);
            Assert.NotNull(info2.Source[0]);

            // Single item
            actual = PipelineReceiverActions.ConvertFromJson(GetJsonContent("Single"), PipelineReceiverActions.PassThru).ToArray();
            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
            Assert.Equal("Test", actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<string>("kind"));

            // Malformed item
            Assert.Throws<PipelineSerializationException>(() => PipelineReceiverActions.ConvertFromJson(new TargetObject(new PSObject("{")), PipelineReceiverActions.PassThru).ToArray());
            Assert.Throws<PipelineSerializationException>(() => PipelineReceiverActions.ConvertFromJson(new TargetObject(new PSObject("{ \"key\": ")), PipelineReceiverActions.PassThru).ToArray());
        }

        [Fact]
        public void DeserializeObjectsMarkdown()
        {
            var actual = PipelineReceiverActions.ConvertFromMarkdown(GetMarkdownContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
            Assert.Equal("Test", actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<string>("kind"));
            Assert.Equal(1, actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<int>("value1"));
            Assert.Equal(2, actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<PSObject[]>("array").Length);
            Assert.Equal("TestObject1", PipelineHookActions.BindTargetName(null, false, false, actual[0].Value));
        }

        [Fact]
        public void DeserializeObjectsPowerShellData()
        {
            var actual = PipelineReceiverActions.ConvertFromPowerShellData(GetDataContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
            Assert.Equal("Test", actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<string>("kind"));
            Assert.Equal(1, actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<int>("value1"));
            Assert.Equal(2, actual[0].Value.PropertyValue("spec").PropertyValue("properties").PropertyValue<Array>("array").Length);
            Assert.Equal("TestObject1", PipelineHookActions.BindTargetName(null, false, false, actual[0].Value));
        }

        #region Helper methods

        private static TargetObject GetYamlContent(string suffix = "")
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ObjectFromFile{suffix}.yaml");
            return new TargetObject(new PSObject(File.ReadAllText(path)));
        }

        private static TargetObject GetJsonContent(string suffix = "")
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ObjectFromFile{suffix}.json");
            return new TargetObject(new PSObject(File.ReadAllText(path)));
        }

        private static TargetObject GetMarkdownContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.md");
            return new TargetObject(new PSObject(File.ReadAllText(path)));
        }

        private static TargetObject GetDataContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.psd1");
            return new TargetObject(new PSObject(File.ReadAllText(path)));
        }

        #endregion Helper methods
    }
}
