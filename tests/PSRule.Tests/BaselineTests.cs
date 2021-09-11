// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;
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

        #region Helper methods

        private Baseline[] GetBaselines(Source[] source)
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var baseline = HostHelper.GetBaselineYaml(source, context).ToArray();
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
