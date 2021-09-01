// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using System;
using System.IO;
using System.Management.Automation;
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

            dynamic configuration = GetConfigurationHelper(option);
            Assert.Equal("value1", configuration.key1);
        }

        [Fact]
        public void GetStringValues()
        {
            var option = new PSRuleOption();
            option.Configuration.Add("key1", "123");
            option.Configuration.Add("key2", new string[] { "123" });
            option.Configuration.Add("key3", new object[] { "123", 456 });

            var configuration = GetConfigurationHelper(option);
            Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key1"));
            Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key2"));
            Assert.Equal(new string[] { "123", "456" }, configuration.GetStringValues("key3"));
        }

        [Fact]
        public void GetStringValuesFromYaml()
        {
            var option = GetOption();
            var actual = option.Configuration["option5"] as Array;
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.IsType<PSObject>(actual.GetValue(0));
            var pso = actual.GetValue(0) as PSObject;
            Assert.Equal("option5a", pso.BaseObject);

            var configuration = GetConfigurationHelper(option);
            Assert.Equal(new string[] { "option5a", "option5b" }, configuration.GetStringValues("option5"));
        }

        [Fact]
        public void GetObjectArrayFromYaml()
        {
            var option = GetOption();
            var actual = option.Configuration["option4"] as Array;
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.IsType<PSObject>(actual.GetValue(0));
            var pso = actual.GetValue(0) as PSObject;
            Assert.Equal("East US", pso.PropertyValue<string>("location"));
        }

        private static Runtime.Configuration GetConfigurationHelper(PSRuleOption option)
        {
            var builder = new OptionContextBuilder(option);
            var context = new PSRule.Runtime.RunspaceContext(PipelineContext.New(option, null, null, null, null, null, builder.Build(), null), null);
            context.Init(null);
            context.Begin();
            return new Runtime.Configuration(context);
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
            return builder.Build();
        }

        private static PSRuleOption GetOption()
        {
            var loaded = PSRuleOption.FromFile(GetSourcePath("PSRule.Tests.yml"));
            var option = new PSRuleOption();
            option.Configuration = loaded.Configuration;
            return option;
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
