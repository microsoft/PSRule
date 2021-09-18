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
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var selector = HostHelper.GetSelectorYaml(GetSource(), context).ToArray();
            Assert.NotNull(selector);
            Assert.Equal(33, selector.Length);

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
            var actual5 = GetObject();

            Assert.True(match.Match(actual1));
            Assert.True(match.Match(actual2));
            Assert.False(match.Match(actual3));
            Assert.False(match.Match(actual4));
            Assert.False(match.Match(actual5));
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
            var actual6 = GetObject();

            Assert.True(@in.Match(actual1));
            Assert.True(@in.Match(actual2));
            Assert.False(@in.Match(actual3));
            Assert.False(@in.Match(actual4));
            Assert.False(@in.Match(actual5));
            Assert.False(@in.Match(actual6));
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
        public void SetOfExpression()
        {
            var setOf = GetSelectorVisitor("YamlSetOf");
            var actual1 = GetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-apiserver", "kube-scheduler" }));
            var actual2 = GetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler" }));
            var actual3 = GetObject((name: "value", value: new string[] { "cluster-autoscaler" }));
            var actual4 = GetObject((name: "value", value: new string[] { "kube-apiserver", "kube-scheduler" }));
            var actual5 = GetObject((name: "value", value: new string[] { "kube-scheduler" }));
            var actual6 = GetObject((name: "value", value: new string[] { }));
            var actual7 = GetObject((name: "value", value: null));
            var actual8 = GetObject();
            var actual9 = GetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler", "kube-apiserver" }));
            var actual10 = GetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-APIserver" }));

            Assert.False(setOf.Match(actual1));
            Assert.True(setOf.Match(actual2));
            Assert.False(setOf.Match(actual3));
            Assert.False(setOf.Match(actual4));
            Assert.False(setOf.Match(actual5));
            Assert.False(setOf.Match(actual6));
            Assert.False(setOf.Match(actual7));
            Assert.False(setOf.Match(actual8));
            Assert.False(setOf.Match(actual9));
            Assert.False(setOf.Match(actual10));
        }

        [Fact]
        public void SubsetExpression()
        {
            var subset = GetSelectorVisitor("YamlSubset");
            var actual1 = GetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-apiserver", "kube-scheduler" }));
            var actual2 = GetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler" }));
            var actual3 = GetObject((name: "value", value: new string[] { "cluster-autoscaler" }));
            var actual4 = GetObject((name: "value", value: new string[] { "kube-apiserver", "kube-scheduler" }));
            var actual5 = GetObject((name: "value", value: new string[] { "kube-scheduler" }));
            var actual6 = GetObject((name: "value", value: new string[] { }));
            var actual7 = GetObject((name: "value", value: null));
            var actual8 = GetObject();
            var actual9 = GetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler", "kube-apiserver" }));
            var actual10 = GetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-APIserver" }));

            Assert.True(subset.Match(actual1));
            Assert.True(subset.Match(actual2));
            Assert.False(subset.Match(actual3));
            Assert.False(subset.Match(actual4));
            Assert.False(subset.Match(actual5));
            Assert.False(subset.Match(actual6));
            Assert.False(subset.Match(actual7));
            Assert.False(subset.Match(actual8));
            Assert.False(subset.Match(actual9));
            Assert.False(subset.Match(actual10));
        }

        [Fact]
        public void CountExpression()
        {
            var count = GetSelectorVisitor("YamlCount");
            var actual1 = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
            var actual2 = GetObject((name: "value", value: new string[] { "2", "1" }));
            var actual3 = GetObject((name: "value", value: new string[] { "1" }));
            var actual4 = GetObject((name: "value", value: new int[] { 2, 3 }));
            var actual5 = GetObject((name: "value", value: new int[] { 3 }));
            var actual6 = GetObject((name: "value", value: new string[] { }));
            var actual7 = GetObject((name: "value", value: null));
            var actual8 = GetObject();

            Assert.False(count.Match(actual1));
            Assert.True(count.Match(actual2));
            Assert.False(count.Match(actual3));
            Assert.True(count.Match(actual4));
            Assert.False(count.Match(actual5));
            Assert.False(count.Match(actual6));
            Assert.False(count.Match(actual7));
            Assert.False(count.Match(actual8));
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

        [Fact]
        public void StartsWithExpression()
        {
            var startsWith = GetSelectorVisitor("YamlStartsWith");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "efg"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject();

            Assert.True(startsWith.Match(actual1));
            Assert.True(startsWith.Match(actual2));
            Assert.False(startsWith.Match(actual3));
            Assert.False(startsWith.Match(actual4));
            Assert.False(startsWith.Match(actual5));
            Assert.False(startsWith.Match(actual6));
        }

        [Fact]
        public void EndsWithExpression()
        {
            var endsWith = GetSelectorVisitor("YamlEndsWith");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "efg"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject();

            Assert.True(endsWith.Match(actual1));
            Assert.True(endsWith.Match(actual2));
            Assert.False(endsWith.Match(actual3));
            Assert.False(endsWith.Match(actual4));
            Assert.False(endsWith.Match(actual5));
            Assert.False(endsWith.Match(actual6));
        }

        [Fact]
        public void ContainsExpression()
        {
            var contains = GetSelectorVisitor("YamlContains");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "bcd"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: new string[] { }));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject();

            Assert.True(contains.Match(actual1));
            Assert.True(contains.Match(actual2));
            Assert.False(contains.Match(actual3));
            Assert.False(contains.Match(actual4));
            Assert.False(contains.Match(actual5));
            Assert.False(contains.Match(actual6));
        }

        [Fact]
        public void IsStringExpression()
        {
            var isStringTrue = GetSelectorVisitor("YamlIsStringTrue");
            var isStringFalse = GetSelectorVisitor("YamlIsStringFalse");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: 4));
            var actual3 = GetObject((name: "value", value: new string[] { }));
            var actual4 = GetObject((name: "value", value: null));
            var actual5 = GetObject();

            // isString: true
            Assert.True(isStringTrue.Match(actual1));
            Assert.False(isStringTrue.Match(actual2));
            Assert.False(isStringTrue.Match(actual3));
            Assert.False(isStringTrue.Match(actual4));
            Assert.False(isStringTrue.Match(actual5));

            // isString: false
            Assert.False(isStringFalse.Match(actual1));
            Assert.True(isStringFalse.Match(actual2));
            Assert.True(isStringFalse.Match(actual3));
            Assert.True(isStringFalse.Match(actual4));
            Assert.False(isStringFalse.Match(actual5));
        }

        [Fact]
        public void IsLowerExpression()
        {
            var isLowerTrue = GetSelectorVisitor("YamlIsLowerTrue");
            var isLowerFalse = GetSelectorVisitor("YamlIsLowerFalse");
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "aBc"));
            var actual3 = GetObject((name: "value", value: "a-b-c"));
            var actual4 = GetObject((name: "value", value: 4));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject();

            // isLower: true
            Assert.True(isLowerTrue.Match(actual1));
            Assert.False(isLowerTrue.Match(actual2));
            Assert.True(isLowerTrue.Match(actual3));
            Assert.False(isLowerTrue.Match(actual4));
            Assert.False(isLowerTrue.Match(actual5));
            Assert.False(isLowerTrue.Match(actual6));

            // isLower: false
            Assert.False(isLowerFalse.Match(actual1));
            Assert.True(isLowerFalse.Match(actual2));
            Assert.False(isLowerFalse.Match(actual3));
            Assert.True(isLowerFalse.Match(actual4));
            Assert.True(isLowerFalse.Match(actual5));
            Assert.False(isLowerTrue.Match(actual6));
        }

        [Fact]
        public void IsUpperExpression()
        {
            var isUpperTrue = GetSelectorVisitor("YamlIsUpperTrue");
            var isUpperFalse = GetSelectorVisitor("YamlIsUpperFalse");
            var actual1 = GetObject((name: "value", value: "ABC"));
            var actual2 = GetObject((name: "value", value: "aBc"));
            var actual3 = GetObject((name: "value", value: "A-B-C"));
            var actual4 = GetObject((name: "value", value: 4));
            var actual5 = GetObject((name: "value", value: null));
            var actual6 = GetObject();

            // isUpper: true
            Assert.True(isUpperTrue.Match(actual1));
            Assert.False(isUpperTrue.Match(actual2));
            Assert.True(isUpperTrue.Match(actual3));
            Assert.False(isUpperTrue.Match(actual4));
            Assert.False(isUpperTrue.Match(actual5));
            Assert.False(isUpperTrue.Match(actual6));

            // isUpper: false
            Assert.False(isUpperFalse.Match(actual1));
            Assert.True(isUpperFalse.Match(actual2));
            Assert.False(isUpperFalse.Match(actual3));
            Assert.True(isUpperFalse.Match(actual4));
            Assert.True(isUpperFalse.Match(actual5));
            Assert.False(isUpperFalse.Match(actual6));
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
            var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var selector = HostHelper.GetSelectorYaml(GetSource(), context).ToArray();
            return new SelectorVisitor(name, selector.FirstOrDefault(s => s.Name == name).Spec.If);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
