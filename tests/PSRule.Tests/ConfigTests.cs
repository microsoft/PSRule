// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace PSRule
{
    public sealed class ConfigTests
    {
        [Fact]
        public void ReadModuleConfig()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, new OptionContext(), null), null);
            var configuration = HostHelper.GetModuleConfig(GetSource(), context).ToArray();
            Assert.NotNull(configuration);
            Assert.Equal("Configuration1", configuration[0].Name);
        }

        private PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("ModuleConfig.Rule.yaml"));
            return builder.Build();
        }

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
