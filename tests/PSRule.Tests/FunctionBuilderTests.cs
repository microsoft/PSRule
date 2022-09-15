// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using PSRule.Configuration;
using PSRule.Definitions.Selectors;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class FunctionBuilderTests
    {
        private const string FunctionYamlFileName = "Functions.Rule.yaml";
        private const string FunctionJsonFileName = "Functions.Rule.jsonc";

        [Theory]
        [InlineData("Yaml", FunctionYamlFileName)]
        [InlineData("Json", FunctionJsonFileName)]
        public void Build(string type, string path)
        {
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example1", GetSource(path), out _));
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example2", GetSource(path), out _));
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example3", GetSource(path), out _));
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example4", GetSource(path), out _));
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example5", GetSource(path), out _));
            Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example6", GetSource(path), out _));
        }

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource(string path)
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath(path));
            return builder.Build();
        }

        private static SelectorVisitor GetSelectorVisitor(string name, Source[] source, out RunspaceContext context)
        {
            context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), null);
            context.Init(source);
            context.Begin();
            var selector = HostHelper.GetSelector(source, context).ToArray().FirstOrDefault(s => s.Name == name);
            return new SelectorVisitor(context, selector.Id, selector.Source, selector.Spec.If);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
