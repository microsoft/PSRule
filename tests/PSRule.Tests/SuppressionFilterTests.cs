// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule
{
    public sealed class SuppressionFilterTests
    {
        [Fact]
        public void Match()
        {
            var option = GetOption();
            var context = new RunspaceContext(PipelineContext.New(option, null, null, null, null, null, new OptionContextBuilder(), null), new TestWriter(option));
            context.Init(GetSource());
            context.Begin();
            var rules = HostHelper.GetRule(GetSource(), context, includeDependencies: false);
            var resourceIndex = new ResourceIndex(rules);
            var filter = new SuppressionFilter(context, option.Suppression, resourceIndex);

            Assert.True(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));
            Assert.False(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));
            Assert.True(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));

            Assert.True(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));
            Assert.True(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));
            Assert.False(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));

            Assert.False(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
            Assert.True(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
            Assert.True(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
        }

        #region Helper methods

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFileAlias.Rule.yaml"));
            builder.Directory(GetSourcePath("FromFileAlias.Rule.jsonc"));
            builder.Directory(GetSourcePath("FromFileAlias.Rule.ps1"));
            return builder.Build();
        }

        private static PSRuleOption GetOption(string path = "PSRule.Tests14.yml")
        {
            return PSRuleOption.FromFileOrEmpty(path);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
