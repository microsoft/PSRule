// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class SuppressionGroupTests
    {
        [Fact]
        public void ReadSuppressionGroup()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var suppressionGroup = HostHelper.GetSuppressionGroupYaml(GetSource(), context).ToArray();
            Assert.NotNull(suppressionGroup);
            Assert.Equal(3, suppressionGroup.Length);
            Assert.Equal("SuppressWithTargetName", suppressionGroup[0].Name);
            Assert.Equal("SuppressWithTestType", suppressionGroup[1].Name);
            Assert.Equal("SuppressWithNonProdTag", suppressionGroup[2].Name);
        }

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource(string path = "SuppressionGroups.Rule.yaml")
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