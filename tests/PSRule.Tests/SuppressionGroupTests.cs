// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
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
        [Theory]
        [InlineData("SuppressionGroups.Rule.yaml")]
        [InlineData("SuppressionGroups.Rule.jsonc")]
        public void ReadSuppressionGroup(string path)
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, GetOptionContext(), null), null);
            context.Init(GetSource(path));
            context.Begin();
            var suppressionGroup = HostHelper.GetSuppressionGroup(GetSource(path), context).ToArray();
            Assert.NotNull(suppressionGroup);
            Assert.Equal(5, suppressionGroup.Length);

            var actual = suppressionGroup[0];
            Assert.Equal("SuppressWithTargetName", actual.Name);
            Assert.Equal("Ignore test objects by name.", actual.Info.Synopsis.Text);
            Assert.Null(actual.Spec.ExpiresOn);
            Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithTargetName"));

            actual = suppressionGroup[1];
            Assert.Equal("SuppressWithTestType", actual.Name);
            Assert.Equal("Ignore test objects by type.", actual.Info.Synopsis.Text);
            Assert.Null(actual.Spec.ExpiresOn);
            Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithTestType"));

            actual = suppressionGroup[2];
            Assert.Equal("SuppressWithNonProdTag", actual.Name);
            Assert.Equal("Ignore objects with non-production tag.", actual.Info.Synopsis.Text);
            Assert.Null(actual.Spec.ExpiresOn);
            Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithNonProdTag"));

            actual = suppressionGroup[3];
            Assert.Equal("SuppressWithExpiry", actual.Name);
            Assert.Equal("Suppress with expiry.", actual.Info.Synopsis.Text);
            Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z").ToUniversalTime(), actual.Spec.ExpiresOn);
            Assert.DoesNotContain(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithExpiry"));

            actual = suppressionGroup[4];
            Assert.Equal("SuppressByScope", actual.Name);
            Assert.Equal("Suppress by scope.", actual.Info.Synopsis.Text);
        }

        //[Theory]
        //[InlineData("SuppressionGroups.Rule.yaml")]
        //[InlineData("SuppressionGroups.Rule.jsonc")]
        //public void EvaluateSuppressionGroup(string path)
        //{
        //    var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, GetOptionContext(), null), null);
        //    context.Init(GetSource(path));
        //    context.Begin();
        //    var suppressionGroup = HostHelper.GetSuppressionGroup(GetSource(path), context).ToArray();
        //    Assert.NotNull(suppressionGroup);

        //    var testObject = GetObject((name: "name", value: "TestObject1"));
        //    context.EnterTargetObject(new TargetObject(testObject, targetName: "TestObject1", scope: "/scope1"));

        //    var actual = suppressionGroup[0];
        //    var visitor = new SuppressionGroupVisitor(context, actual.Id, actual.Source, actual.Spec, actual.Info);
        //    Assert.True(visitor.TryMatch(testObject, out _));

        //    actual = suppressionGroup[4];
        //    visitor = new SuppressionGroupVisitor(context, actual.Id, actual.Source, actual.Spec, actual.Info);
        //    //Assert.True(visitor.TryMatch());
        //}

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            var option = new PSRuleOption();
            option.Output.Culture = new string[] { "en-US", "en" };
            return option;
        }

        private static OptionContext GetOptionContext()
        {
            return new OptionContextBuilder(GetOption(), null, null, null).Build();
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

        private static PSObject GetObject(params (string name, object value)[] properties)
        {
            var result = new PSObject();
            for (var i = 0; properties != null && i < properties.Length; i++)
                result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

            return result;
        }

        #endregion Helper methods
    }
}
