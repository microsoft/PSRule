// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using System;
using System.IO;
using System.Management.Automation;
using Xunit;
using static PSRule.PipelineTests;

namespace PSRule
{
    public sealed class ConventionTests
    {
        [Fact]
        public void WithConventions()
        {
            var testObject1 = new TestObject { Name = "TestObject1" };
            var option = GetOption();
            option.Rule.Include = new string[] { "ConventionTest" };
            option.Convention.Include = new string[] { "Convention1" };
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            var pipeline = builder.Build();
            pipeline.Begin();
            pipeline.Process(PSObject.AsPSObject(testObject1));
            pipeline.End();
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFileConventions.Rule.ps1"));
            return builder.Build();
        }

        private PSRuleOption GetOption(string path = null)
        {
            if (path == null)
                return new PSRuleOption();

            return PSRuleOption.FromFile(path);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
