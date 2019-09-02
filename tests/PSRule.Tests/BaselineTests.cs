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
    public sealed class BaselineTests
    {
        [Fact]
        public void ReadBaseline()
        {
            var context = PipelineContext.New(null, GetOption(), null, new BaselineContext(), null);
            var baseline = HostHelper.GetBaseline(GetSource(), context).ToArray();
            Assert.NotNull(baseline);
            Assert.Equal("TestBaseline1", baseline[0].Name);
        }

        private PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private Source[] GetSource()
        {
            var builder = new RuleSourceBuilder();
            builder.Directory(GetSourcePath("Baseline.Rule.yaml"));
            return builder.Build();
        }

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
