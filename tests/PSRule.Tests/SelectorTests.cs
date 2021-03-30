// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Selectors;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;
using Assert = Xunit.Assert;

namespace PSRule
{
    public sealed class SelectorTests
    {
        [Fact]
        public void ReadSelector()
        {
            var context = PipelineContext.New(GetOption(), null, null, null, new OptionContext(), null);
            var selector = HostHelper.GetSelector(GetSource(), new RunspaceContext(context, null)).ToArray();
            Assert.NotNull(selector);
            Assert.Equal(21, selector.Length);

            Assert.Equal("BasicSelector", selector[0].Name);
            Assert.Equal("YamlAllOf", selector[4].Name);
        }

        #region Conditions

        [Fact]
        public void ExistsExpression()
        {
            var existsTrue = GetSelectorVisitor("YamlExistsTrue");
            var existsFalse = GetSelectorVisitor("YamlExistsFalse");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "notValue", value: 3));
            var actual3 = GetObject((name: "value", value: null));

            Assert.True(existsTrue.Match(actual1));
            Assert.False(existsTrue.Match(actual2));
            Assert.True(existsTrue.Match(actual3));

            Assert.False(existsFalse.Match(actual1));
            Assert.True(existsFalse.Match(actual2));
            Assert.False(existsFalse.Match(actual3));
        }

        [Fact]
        public void EqualsExpression()
        {
            var equals = GetSelectorVisitor("YamlEquals");
            var actual1 = GetObject(
                (name: "ValueString", value: "abc"),
                (name: "ValueInt", value: 123),
                (name: "ValueBool", value: true)
            );
            var actual2 = GetObject(
                (name: "ValueString", value: "efg"),
                (name: "ValueInt", value: 123),
                (name: "ValueBool", value: true)
            );
            var actual3 = GetObject(
                (name: "ValueString", value: "abc"),
                (name: "ValueInt", value: 456),
                (name: "ValueBool", value: true)
            );
            var actual4 = GetObject(
                (name: "ValueString", value: "abc"),
                (name: "ValueInt", value: 123),
                (name: "ValueBool", value: false)
            );

            Assert.True(equals.Match(actual1));
            Assert.False(equals.Match(actual2));
            Assert.False(equals.Match(actual3));
            Assert.False(equals.Match(actual4));
        }

        [Fact]
        public void NotEqualsExpression()
        {
            var notEquals = GetSelectorVisitor("YamlNotEquals");
            var actual1 = GetObject(
                (name: "ValueString", value: "efg"),
                (name: "ValueInt", value: 456),
                (name: "ValueBool", value: false)
            );
            var actual2 = GetObject(
                (name: "ValueString", value: "abc"),
                (name: "ValueInt", value: 456),
                (name: "ValueBool", value: false)
            );
            var actual3 = GetObject(
                (name: "ValueString", value: "efg"),
                (name: "ValueInt", value: 123),
                (name: "ValueBool", value: false)
            );
            var actual4 = GetObject(
                (name: "ValueString", value: "efg"),
                (name: "ValueInt", value: 456),
                (name: "ValueBool", value: true)
            );

            Assert.True(notEquals.Match(actual1));
            Assert.False(notEquals.Match(actual2));
            Assert.False(notEquals.Match(actual3));
            Assert.False(notEquals.Match(actual4));
        }

        [Fact]
        public void HasValueExpression()
        {
            var hasValueTrue = GetSelectorVisitor("YamlHasValueTrue");
            var hasValueFalse = GetSelectorVisitor("YamlHasValueFalse");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "notValue", value: 3));
            var actual3 = GetObject((name: "value", value: null));

            Assert.True(hasValueTrue.Match(actual1));
            Assert.False(hasValueTrue.Match(actual2));
            Assert.False(hasValueTrue.Match(actual3));

            Assert.False(hasValueFalse.Match(actual1));
            Assert.True(hasValueFalse.Match(actual2));
            Assert.True(hasValueFalse.Match(actual3));
        }

        [Fact]
        public void MatchExpression()
        {
            var match = GetSelectorVisitor("YamlMatch");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "efg"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: 0));

            Assert.True(match.Match(actual1));
            Assert.True(match.Match(actual2));
            Assert.False(match.Match(actual3));
            Assert.False(match.Match(actual4));
        }

        [Fact]
        public void NotMatchExpression()
        {
            var notMatch = GetSelectorVisitor("YamlNotMatch");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "efg"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: 0));

            Assert.False(notMatch.Match(actual1));
            Assert.False(notMatch.Match(actual2));
            Assert.True(notMatch.Match(actual3));
            Assert.True(notMatch.Match(actual4));
        }

        [Fact]
        public void InExpression()
        {
            var @in = GetSelectorVisitor("YamlIn");
            var actual1 = GetObject((name: "value", value: new string[] { "Value1" }));
            var actual2 = GetObject((name: "value", value: new string[] { "Value2" }));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));

            Assert.True(@in.Match(actual1));
            Assert.True(@in.Match(actual2));
            Assert.False(@in.Match(actual3));
            Assert.False(@in.Match(actual4));
            Assert.False(@in.Match(actual5));
        }

        [Fact]
        public void NotInExpression()
        {
            var notIn = GetSelectorVisitor("YamlNotIn");
            var actual1 = GetObject((name: "value", value: new string[] { "Value1" }));
            var actual2 = GetObject((name: "value", value: new string[] { "Value2" }));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));

            Assert.False(notIn.Match(actual1));
            Assert.False(notIn.Match(actual2));
            Assert.True(notIn.Match(actual3));
            Assert.True(notIn.Match(actual4));
            Assert.True(notIn.Match(actual5));
        }

        [Fact]
        public void LessExpression()
        {
            var less = GetSelectorVisitor("YamlLess");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "value", value: 4));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject((name: "value", value: 2));
            var actual7 = GetObject((name: "value", value: -1));

            Assert.False(less.Match(actual1));
            Assert.False(less.Match(actual2));
            Assert.True(less.Match(actual3));
            Assert.True(less.Match(actual4));
            Assert.True(less.Match(actual5));
            Assert.True(less.Match(actual6));
            Assert.True(less.Match(actual7));
        }

        [Fact]
        public void LessOrEqualsExpression()
        {
            var lessOrEquals = GetSelectorVisitor("YamlLessOrEquals");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "value", value: 4));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject((name: "value", value: 2));
            var actual7 = GetObject((name: "value", value: -1));

            Assert.True(lessOrEquals.Match(actual1));
            Assert.False(lessOrEquals.Match(actual2));
            Assert.True(lessOrEquals.Match(actual3));
            Assert.True(lessOrEquals.Match(actual4));
            Assert.True(lessOrEquals.Match(actual5));
            Assert.True(lessOrEquals.Match(actual6));
            Assert.True(lessOrEquals.Match(actual7));
        }

        [Fact]
        public void GreaterExpression()
        {
            var greater = GetSelectorVisitor("YamlGreater");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "value", value: 4));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject((name: "value", value: 2));
            var actual7 = GetObject((name: "value", value: -1));

            Assert.False(greater.Match(actual1));
            Assert.True(greater.Match(actual2));
            Assert.False(greater.Match(actual3));
            Assert.False(greater.Match(actual4));
            Assert.False(greater.Match(actual5));
            Assert.False(greater.Match(actual6));
            Assert.False(greater.Match(actual7));
        }

        [Fact]
        public void GreaterOrEqualsExpression()
        {
            var greaterOrEquals = GetSelectorVisitor("YamlGreaterOrEquals");
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "value", value: 4));
            var actual3 = GetObject((name: "value", value: new string[] { "Value3" }));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject((name: "value", value: 2));
            var actual7 = GetObject((name: "value", value: -1));

            Assert.True(greaterOrEquals.Match(actual1));
            Assert.True(greaterOrEquals.Match(actual2));
            Assert.False(greaterOrEquals.Match(actual3));
            Assert.False(greaterOrEquals.Match(actual4));
            Assert.False(greaterOrEquals.Match(actual5));
            Assert.False(greaterOrEquals.Match(actual6));
            Assert.False(greaterOrEquals.Match(actual7));
        }

        #endregion Conditions

        #region Operators

        [Fact]
        public void AllOf()
        {
            var allOf = GetSelectorVisitor("YamlAllOf");
            var actual1 = GetObject((name: "Name", value: "Name1"));
            var actual2 = GetObject((name: "AlternateName", value: "Name2"));
            var actual3 = GetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
            var actual4 = GetObject((name: "OtherName", value: "Name3"));

            Assert.False(allOf.Match(actual1));
            Assert.False(allOf.Match(actual2));
            Assert.True(allOf.Match(actual3));
            Assert.False(allOf.Match(actual4));
        }

        [Fact]
        public void AnyOf()
        {
            var allOf = GetSelectorVisitor("YamlAnyOf");
            var actual1 = GetObject((name: "Name", value: "Name1"));
            var actual2 = GetObject((name: "AlternateName", value: "Name2"));
            var actual3 = GetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
            var actual4 = GetObject((name: "OtherName", value: "Name3"));

            Assert.True(allOf.Match(actual1));
            Assert.True(allOf.Match(actual2));
            Assert.True(allOf.Match(actual3));
            Assert.False(allOf.Match(actual4));
        }

        [Fact]
        public void Not()
        {
            var allOf = GetSelectorVisitor("YamlNot");
            var actual1 = GetObject((name: "Name", value: "Name1"));
            var actual2 = GetObject((name: "AlternateName", value: "Name2"));
            var actual3 = GetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
            var actual4 = GetObject((name: "OtherName", value: "Name3"));

            Assert.False(allOf.Match(actual1));
            Assert.False(allOf.Match(actual2));
            Assert.False(allOf.Match(actual3));
            Assert.True(allOf.Match(actual4));
        }

        #endregion Operators

        #region Helper methods

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("Selectors.Rule.yaml"));
            return builder.Build();
        }

        private static PSObject GetObject(params (string name, object value)[] properties)
        {
            var result = new PSObject();
            for (var i = 0; properties != null && i < properties.Length; i++)
                result.Properties.Add(new PSNoteProperty(properties[i].Item1, properties[i].Item2));

            return result;
        }

        private static SelectorVisitor GetSelectorVisitor(string name)
        {
            var context = PipelineContext.New(GetOption(), null, null, null, new OptionContext(), null);
            var selector = HostHelper.GetSelector(GetSource(), new RunspaceContext(context, null)).ToArray();
            return new SelectorVisitor(name, selector.FirstOrDefault(s => s.Name == name).Spec.If);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
