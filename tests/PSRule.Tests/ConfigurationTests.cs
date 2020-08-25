// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.IO;
using Xunit;

namespace PSRule
{
    [Trait(LANGUAGE, LANGUAGEELEMENT)]
    public sealed class ConfigurationTests
    {
        private const string LANGUAGE = "Language";
        private const string LANGUAGEELEMENT = "Variable";

        [Fact]
        public void Configuration()
        {
            var option = new PSRuleOption();
            option.Configuration.Add("key1", "value1");
            BuildPipeline(option);

            dynamic configuration = GetConfigurationHelper();
            Assert.Equal("value1", configuration.key1);
        }

        [Fact]
        public void GetStringValues()
        {
            var option = new PSRuleOption();
            option.Configuration.Add("key1", "123");
            option.Configuration.Add("key2", new string[] { "123" });
            option.Configuration.Add("key3", new object[] { "123", 456 });
            BuildPipeline(option);

            var configuration = GetConfigurationHelper();
            Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key1"));
            Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key2"));
            Assert.Equal(new string[] { "123", "456" }, configuration.GetStringValues("key3"));
        }

        private static void BuildPipeline(PSRuleOption option)
        {
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            builder.Build();
        }

        private static Runtime.Configuration GetConfigurationHelper()
        {
            return new Runtime.Configuration();
        }

        private static Source[] GetSource()
        {
            var builder = new RuleSourceBuilder(null);
            builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
