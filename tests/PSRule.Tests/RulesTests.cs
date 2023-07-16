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
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class RulesTests
    {
        #region Yaml rules

        /// <summary>
        /// Test that a YAML-based rule can be parsed.
        /// </summary>
        [Fact]
        public void ReadYamlRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource());
            context.Begin();

            // From current path
            var rule = HostHelper.GetRule(GetSource(), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("YamlBasicRule", rule[0].Name);
            Assert.Equal(Environment.GetRootedPath(""), rule[0].Source.HelpPath);
            Assert.Equal(10, rule[0].Extent.Line);

            // From relative path
            rule = HostHelper.GetRule(GetSource("../../../FromFile.Rule.yaml"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("YamlBasicRule", rule[0].Name);
            Assert.Equal(Environment.GetRootedPath("../../.."), rule[0].Source.HelpPath);

            var hashtable = rule[0].Tag.ToHashtable();
            Assert.Equal("tag", hashtable["feature"]);

            var block = HostHelper.GetRuleBlockGraph(GetSource(), context).GetAll();
            var actual = block.FirstOrDefault(b => b.Name == "YamlBasicRule");
            Assert.NotNull(actual.Info.Annotations);
            Assert.Equal("test123", actual.Info.Annotations["test_value"]);
            Assert.Equal("Basic YAML rule", actual.Info.DisplayName);
            Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
            Assert.Equal("A YAML rule recommendation for testing.", actual.Info.Recommendation);
            Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
        }

        /// <summary>
        /// Test that a YAML-based rule with sub-selectors can be parsed.
        /// </summary>
        [Fact]
        public void ReadYamlSubSelectorRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource("FromFileSubSelector.Rule.yaml"));
            context.Begin();

            // From current path
            var rule = HostHelper.GetRule(GetSource("FromFileSubSelector.Rule.yaml"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("YamlRuleWithPrecondition", rule[0].Name);
            Assert.Equal("YamlRuleWithSubselector", rule[1].Name);
            Assert.Equal("YamlRuleWithSubselectorReordered", rule[2].Name);
            Assert.Equal("YamlRuleWithQuantifier", rule[3].Name);

            context.Init(GetSource("FromFileSubSelector.Rule.yaml"));
            context.Begin();
            var subselector1 = GetRuleVisitor(context, "YamlRuleWithPrecondition", GetSource("FromFileSubSelector.Rule.yaml"));
            var subselector2 = GetRuleVisitor(context, "YamlRuleWithSubselector", GetSource("FromFileSubSelector.Rule.yaml"));
            var subselector3 = GetRuleVisitor(context, "YamlRuleWithSubselectorReordered", GetSource("FromFileSubSelector.Rule.yaml"));
            var subselector4 = GetRuleVisitor(context, "YamlRuleWithQuantifier", GetSource("FromFileSubSelector.Rule.yaml"));
            context.EnterLanguageScope(subselector1.Source);

            var actual1 = GetObject((name: "kind", value: "test"), (name: "resources", value: new string[] { "abc", "abc" }));
            var actual2 = GetObject((name: "resources", value: new string[] { "abc", "123", "abc" }));

            // YamlRuleWithPrecondition
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector1);
            Assert.True(subselector1.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector1);
            Assert.True(subselector1.Condition.If().Skipped());

            // YamlRuleWithSubselector
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector2);
            Assert.True(subselector2.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector2);
            Assert.False(subselector2.Condition.If().AllOf());

            // YamlRuleWithSubselectorReordered
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector3);
            Assert.True(subselector3.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector3);
            Assert.True(subselector3.Condition.If().AllOf());

            // YamlRuleWithQuantifier
            var fromFile = GetObjectAsTarget("ObjectFromFile3.json");
            actual1 = fromFile[0];
            actual2 = fromFile[1];
            var actual3 = fromFile[2];

            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector4);
            Assert.True(subselector4.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector4);
            Assert.False(subselector4.Condition.If().AllOf());

            context.EnterTargetObject(actual3);
            context.EnterRuleBlock(subselector4);
            Assert.True(subselector4.Condition.If().AllOf());
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
            var customType = GetRuleVisitor(context, "RuleYamlWithCustomType");
            var withSelector = GetRuleVisitor(context, "RuleYamlWithSelector");
            context.EnterLanguageScope(yamlTrue.Source);

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
            context.EnterLanguageScope(yamlObjectPath.Source);

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

        #endregion Yaml rules

        #region Json rules

        /// <summary>
        /// Test that a JSON-based rule can be parsed.
        /// </summary>
        [Fact]
        public void ReadJsonRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource());
            context.Begin();

            // From current path
            var rule = HostHelper.GetRule(GetSource("FromFile.Rule.jsonc"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("JsonBasicRule", rule[0].Name);
            Assert.Equal(Environment.GetRootedPath(""), rule[0].Source.HelpPath);
            Assert.Equal(7, rule[0].Extent.Line);

            // From relative path
            rule = HostHelper.GetRule(GetSource("../../../FromFile.Rule.jsonc"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("JsonBasicRule", rule[0].Name);
            Assert.Equal(Environment.GetRootedPath("../../.."), rule[0].Source.HelpPath);

            var hashtable = rule[0].Tag.ToHashtable();
            Assert.Equal("tag", hashtable["feature"]);

            var block = HostHelper.GetRuleBlockGraph(GetSource("FromFile.Rule.jsonc"), context).GetAll();
            var actual = block.FirstOrDefault(b => b.Name == "JsonBasicRule");
            Assert.NotNull(actual.Info.Annotations);
            Assert.Equal("test123", actual.Info.Annotations["test_value"]);
            Assert.Equal("Basic JSON rule", actual.Info.DisplayName);
            Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
            Assert.Equal("A JSON rule recommendation for testing.", actual.Info.Recommendation);
            Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
        }

        /// <summary>
        /// Test that a JSON-based rule with sub-selectors can be parsed.
        /// </summary>
        [Fact]
        public void ReadJsonSubSelectorRule()
        {
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), new TestWriter(GetOption()));
            context.Init(GetSource("FromFileSubSelector.Rule.jsonc"));
            context.Begin();

            // From current path
            var rule = HostHelper.GetRule(GetSource("FromFileSubSelector.Rule.jsonc"), context, includeDependencies: false);
            Assert.NotNull(rule);
            Assert.Equal("JsonRuleWithPrecondition", rule[0].Name);
            Assert.Equal("JsonRuleWithSubselector", rule[1].Name);
            Assert.Equal("JsonRuleWithSubselectorReordered", rule[2].Name);
            Assert.Equal("JsonRuleWithQuantifier", rule[3].Name);

            context.Init(GetSource("FromFileSubSelector.Rule.yaml"));
            context.Begin();
            var subselector1 = GetRuleVisitor(context, "JsonRuleWithPrecondition", GetSource("FromFileSubSelector.Rule.jsonc"));
            var subselector2 = GetRuleVisitor(context, "JsonRuleWithSubselector", GetSource("FromFileSubSelector.Rule.jsonc"));
            var subselector3 = GetRuleVisitor(context, "JsonRuleWithSubselectorReordered", GetSource("FromFileSubSelector.Rule.jsonc"));
            var subselector4 = GetRuleVisitor(context, "JsonRuleWithQuantifier", GetSource("FromFileSubSelector.Rule.jsonc"));
            context.EnterLanguageScope(subselector1.Source);

            var actual1 = GetObject((name: "kind", value: "test"), (name: "resources", value: new string[] { "abc", "abc" }));
            var actual2 = GetObject((name: "resources", value: new string[] { "abc", "123", "abc" }));

            // JsonRuleWithPrecondition
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector1);
            Assert.True(subselector1.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector1);
            Assert.True(subselector1.Condition.If().Skipped());

            // JsonRuleWithSubselector
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector2);
            Assert.True(subselector2.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector2);
            Assert.False(subselector2.Condition.If().AllOf());

            // JsonRuleWithSubselectorReordered
            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector3);
            Assert.True(subselector3.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector3);
            Assert.True(subselector3.Condition.If().AllOf());

            // JsonRuleWithQuantifier
            var fromFile = GetObjectAsTarget("ObjectFromFile3.json");
            actual1 = fromFile[0];
            actual2 = fromFile[1];
            var actual3 = fromFile[2];

            context.EnterTargetObject(actual1);
            context.EnterRuleBlock(subselector4);
            Assert.True(subselector4.Condition.If().AllOf());

            context.EnterTargetObject(actual2);
            context.EnterRuleBlock(subselector4);
            Assert.False(subselector4.Condition.If().AllOf());

            context.EnterTargetObject(actual3);
            context.EnterRuleBlock(subselector4);
            Assert.True(subselector4.Condition.If().AllOf());
        }

        #endregion Json rules

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource(string path = "FromFile.Rule.yaml")
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath(path));
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

        private static TargetObject[] GetObjectAsTarget(string path)
        {
            return JsonConvert.DeserializeObject<object[]>(File.ReadAllText(path)).Select(o => new TargetObject(new PSObject(o))).ToArray();
        }

        private static RuleBlock GetRuleVisitor(RunspaceContext context, string name, Source[] source = null)
        {
            var block = HostHelper.GetRuleBlockGraph(source ?? GetSource(), context).GetAll();
            return block.FirstOrDefault(s => s.Name == name);
        }

        private static void ImportSelectors(RunspaceContext context, Source[] source = null)
        {
            var selectors = HostHelper.GetSelectorForTests(source ?? GetSource(), context).ToArray();
            foreach (var selector in selectors)
                context.Pipeline.Import(context, selector);
        }

        private static string GetSourcePath(string path)
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }

        #endregion Helper methods
    }
}
