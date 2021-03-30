// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class BaselineTests
    {
        [Fact]
        public void ReadBaseline()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, new OptionContext(), null), null);
            var baseline = HostHelper.GetBaseline(GetSource(), context).ToArray();
            Assert.NotNull(baseline);
            Assert.Equal(5, baseline.Length);

            // TestBaseline1
            Assert.Equal("TestBaseline1", baseline[0].Name);
            Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
            Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
            Assert.False(baseline[0].Obsolete);
            Assert.False(baseline[0].GetApiVersionIssue());

            // TestBaseline5
            Assert.Equal("TestBaseline5", baseline[4].Name);
            Assert.Equal("github.com/microsoft/PSRule/v1", baseline[4].ApiVersion);
            Assert.True(baseline[4].Obsolete);
            Assert.True(baseline[4].GetApiVersionIssue());
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

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
