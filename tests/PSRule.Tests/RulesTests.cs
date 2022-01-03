// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class RulesTests
    {
        [Fact]
        public void ReadYamlRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource());
            context.Begin();
            var rule = HostHelper.GetRuleYaml(GetSource(), context).ToArray();
            Assert.NotNull(rule);
            Assert.Equal("BasicRule", rule[0].RuleName);

            var hashtable = rule[0].Tag.ToHashtable();
            Assert.Equal("tag", hashtable["feature"]);
        }

        [Fact]
        public void EvaluateYamlRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource());
            context.Begin();
            ImportSelectors(context);
            var yamlTrue = GetRuleVisitor(context, "RuleYamlTrue");
            var yamlFalse = GetRuleVisitor(context, "RuleYamlFalse");
            var customType = GetRuleVisitor(context, "RuleWithCustomType");
            var withSelector = GetRuleVisitor(context, "RuleWithSelector");
            context.EnterSourceScope(yamlTrue.Source);

            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "notValue", value: 3));
            var actual3 = GetObject((name: "value", value: 4));
            actual3.Value.TypeNames.Insert(0, "CustomType");

            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(yamlTrue);
            Assert.True(yamlTrue.Condition.If().AllOf());
            context.EnterRuleBlock(yamlFalse);
            Assert.False(yamlFalse.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(yamlTrue);
            Assert.False(yamlTrue.Condition.If().AllOf());
            context.EnterRuleBlock(yamlFalse);
            Assert.False(yamlFalse.Condition.If().AllOf());

            context.EnterTargetObject(actual3);
            context.EnterRuleBlock(yamlTrue);
            Assert.False(yamlTrue.Condition.If().AllOf());
            context.EnterRuleBlock(yamlFalse);
            Assert.True(yamlFalse.Condition.If().AllOf());

            // With type pre-condition
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(customType);
            Assert.Null(customType.Condition.If());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(customType);
            Assert.Null(customType.Condition.If());

            context.EnterTargetObject(actual3);
            context.EnterRuleBlock(customType);
            Assert.NotNull(customType.Condition.If());

            // With selector pre-condition
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(withSelector);
            Assert.Null(withSelector.Condition.If());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(withSelector);
            Assert.NotNull(withSelector.Condition.If());

            context.EnterTargetObject(actual3);
            context.EnterRuleBlock(withSelector);
            Assert.Null(withSelector.Condition.If());
        }

        [Fact]
        public void RuleWithObjectPath()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource());
            context.Begin();
            ImportSelectors(context);
            var yamlObjectPath = GetRuleVisitor(context, "YamlObjectPath");
            context.EnterSourceScope(yamlObjectPath.Source);

            var actual = GetObject(GetSourcePath("ObjectFromFile3.json"));

            context.EnterTargetObject(new TargetObject(new PSObject(actual[0])));
            context.EnterRuleBlock(yamlObjectPath);
            Assert.True(yamlObjectPath.Condition.If().AllOf());

            context.EnterTargetObject(new TargetObject(new PSObject(actual[1])));
            context.EnterRuleBlock(yamlObjectPath);
            Assert.False(yamlObjectPath.Condition.If().AllOf());

            context.EnterTargetObject(new TargetObject(new PSObject(actual[2])));
            context.EnterRuleBlock(yamlObjectPath);
            Assert.True(yamlObjectPath.Condition.If().AllOf());
        }

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("FromFile.Rule.yaml"));
            return builder.Build();
        }

        private static TargetObject GetObject(params (string name, object value)[] properties)
        {
            var result = new PSObject();
            for (var i = 0; properties != null && i < properties.Length; i++)
                result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

            return new TargetObject(result);
        }

        private static object[] GetObject(string path)
        {
            return JsonConvert.DeserializeObject<object[]>(File.ReadAllText(path));
        }

        private static RuleBlock GetRuleVisitor(RunspaceContext context, string name)
        {
            var block = HostHelper.GetRuleYamlBlocks(GetSource(), context);
            return block.FirstOrDefault(s => s.RuleName == name);
        }

        private static void ImportSelectors(RunspaceContext context)
        {
            var selectors = HostHelper.GetSelectorYaml(GetSource(), context).ToArray();
            foreach (var selector in selectors)
                context.Pipeline.Import(selector);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
