﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Pipeline.Output;
using PSRule.Runtime;
using Xunit;
using YamlDotNet.Serialization;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class BaselineTests
    {
        [Fact]
        public void ReadBaseline()
        {
            var baseline = GetBaselines(GetSource());
            Assert.NotNull(baseline);
            Assert.Equal(5, baseline.Length);

            // TestBaseline1
            Assert.Equal("TestBaseline1", baseline[0].Name);
            Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
            Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
            Assert.False(baseline[0].Obsolete);
            Assert.False(baseline[0].GetApiVersionIssue());

            var config = baseline[0].Spec.Configuration["key2"] as Array;
            Assert.NotNull(config);
            Assert.Equal(2, config.Length);
            Assert.IsType<PSObject>(config.GetValue(0));
            var pso = config.GetValue(0) as PSObject;
            Assert.Equal("abc", pso.PropertyValue<string>("value1"));
            pso = config.GetValue(1) as PSObject;
            Assert.Equal("def", pso.PropertyValue<string>("value2"));

            // TestBaseline5
            Assert.Equal("TestBaseline5", baseline[4].Name);
            Assert.Equal("github.com/microsoft/PSRule/v1", baseline[4].ApiVersion);
            Assert.True(baseline[4].Obsolete);
            Assert.True(baseline[4].GetApiVersionIssue());
        }

        [Fact]
        public void ReadBaselineInModule()
        {
            var baseline = GetBaselines(GetSourceInModule());
            Assert.NotNull(baseline);
            Assert.Equal(5, baseline.Length);

            // TestBaseline1
            Assert.Equal("TestBaseline1", baseline[0].Name);
            Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
            Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
            Assert.False(baseline[0].Obsolete);
            Assert.False(baseline[0].GetApiVersionIssue());
        }

        [Fact]
        public void FilterBaseline()
        {
            var baseline = GetBaselines(GetSource());
            Assert.NotNull(baseline);

            var filter = new BaselineFilter(new string[] { "TestBaseline5" });
            var actual = baseline.FirstOrDefault(b => filter.Match(b));

            Assert.Equal("TestBaseline5", actual.Name);
        }

        [Fact]
        public void BaselineAsYaml()
        {
            var expected = GetBaselines(GetSource());
            var s = YamlOutputWriter.ToYaml(expected);
            var d = new DeserializerBuilder().Build();
            var actual = d.Deserialize<dynamic>(s);

            // TestBaseline1
            Assert.Equal("github.com/microsoft/PSRule/v1", actual[0]["apiVersion"]);
            Assert.Equal("Baseline", actual[0]["kind"]);
            Assert.Equal("TestBaseline1", actual[0]["metadata"]["name"]);
            Assert.NotNull(actual[0]["spec"]);
            Assert.Equal("kind", actual[0]["spec"]["binding"]["field"]["kind"][0]);
            Assert.Equal("Id", actual[0]["spec"]["binding"]["field"]["uniqueIdentifer"][0]);
            Assert.Equal("AlternateName", actual[0]["spec"]["binding"]["field"]["uniqueIdentifer"][1]);
            Assert.Equal("AlternateName", actual[0]["spec"]["binding"]["targetName"][0]);
            Assert.Equal("kind", actual[0]["spec"]["binding"]["targetType"][0]);
            Assert.Equal("WithBaseline", actual[0]["spec"]["rule"]["include"][0]);
            Assert.Equal("value1", actual[0]["spec"]["configuration"]["key1"]);
            Assert.Equal("abc", actual[0]["spec"]["configuration"]["key2"][0]["value1"]);
            Assert.Equal("def", actual[0]["spec"]["configuration"]["key2"][1]["value2"]);
        }

        #region Helper methods

        private Baseline[] GetBaselines(Source[] source)
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var baseline = HostHelper.GetBaseline(source, context).ToArray();
            return baseline;
        }

        private PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("Baseline.Rule.yaml"));
            return builder.Build();
        }

        private Source[] GetSourceInModule()
        {
            var file = new SourceFile(GetSourcePath("Baseline.Rule.yaml"), "TestModule", SourceType.Yaml, null);
            var source = new Source(AppDomain.CurrentDomain.BaseDirectory, new SourceFile[] { file });
            return new Source[] { source };
        }

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
