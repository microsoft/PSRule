// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule
{
    public sealed class ModuleConfigTests
    {
        [Theory]
        [InlineData("ModuleConfig.Rule.yaml")]
        [InlineData("ModuleConfig.Rule.jsonc")]
        public void ReadModuleConfig(string path)
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContextBuilder(), null), null);
            var configuration = HostHelper.GetModuleConfigForTests(GetSource(path), context).ToArray();
            Assert.NotNull(configuration);
            Assert.Equal("Configuration1", configuration[0].Name);
        }

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource(string path)
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath(path));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
