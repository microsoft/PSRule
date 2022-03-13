// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class ResourceValidatorTests
    {
        [Fact]
        public void ResourceName()
        {
            var writer = new TestWriter(GetOption());
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), writer);

            // Get good rules
            var rule = HostHelper.GetRule(GetSource(), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Empty(writer.Errors);

            // Get invalid rule names YAML
            rule = HostHelper.GetRule(GetSource("FromFileName.Rule.yaml"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.NotEmpty(writer.Errors);
            Assert.Equal("PSRule.Parse.InvalidResourceName", writer.Errors[0].FullyQualifiedErrorId);

            // Get invalid rule names JSON
            writer.Errors.Clear();
            rule = HostHelper.GetRule(GetSource("FromFileName.Rule.jsonc"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.NotEmpty(writer.Errors);
            Assert.Equal("PSRule.Parse.InvalidResourceName", writer.Errors[0].FullyQualifiedErrorId);
        }

        #region Helper methods

        private static Source[] GetSource(string path = "FromFile.Rule.yaml")
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath(path));
            return builder.Build();
        }

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
