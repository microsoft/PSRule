// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Definitions.Selectors;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
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
            Assert.Equal(59, selector.Length);

            Assert.Equal("BasicSelector", selector[0].Name);
            Assert.Equal("YamlAllOf", selector[4].Name);
        }

        #region Conditions

        [Fact]
        public void ExistsExpression()
        {
            var existsTrue = GetSelectorVisitor("YamlExistsTrue", out _);
            var existsFalse = GetSelectorVisitor("YamlExistsFalse", out _);
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
            var equals = GetSelectorVisitor("YamlEquals", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameEquals", out RunspaceContext context);
            var actual5 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual6 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual5));
            Assert.True(withName.Match(actual5));

            context.EnterTargetObject(new TargetObject(actual6));
            Assert.False(withName.Match(actual6));

            // With type
            var withType = GetSelectorVisitor("YamlTypeEquals", out context);
            var actual7 = GetObject();
            actual7.TypeNames.Insert(0, "CustomType1");
            var actual8 = GetObject();
            actual8.TypeNames.Insert(0, "CustomType2");

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withType.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withType.Match(actual8));
        }

        [Fact]
        public void NotEqualsExpression()
        {
            var notEquals = GetSelectorVisitor("YamlNotEquals", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameNotEquals", out RunspaceContext context);
            var actual5 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual6 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual5));
            Assert.False(withName.Match(actual5));

            context.EnterTargetObject(new TargetObject(actual6));
            Assert.True(withName.Match(actual6));
        }

        [Fact]
        public void HasValueExpression()
        {
            var hasValueTrue = GetSelectorVisitor("YamlHasValueTrue", out _);
            var hasValueFalse = GetSelectorVisitor("YamlHasValueFalse", out _);
            var actual1 = GetObject((name: "value", value: 3));
            var actual2 = GetObject((name: "notValue", value: 3));
            var actual3 = GetObject((name: "value", value: null));

            Assert.True(hasValueTrue.Match(actual1));
            Assert.False(hasValueTrue.Match(actual2));
            Assert.False(hasValueTrue.Match(actual3));

            Assert.False(hasValueFalse.Match(actual1));
            Assert.True(hasValueFalse.Match(actual2));
            Assert.True(hasValueFalse.Match(actual3));

            // With name
            var withName = GetSelectorVisitor("YamlNameHasValue", out RunspaceContext context);
            var actual4 = GetObject();

            context.EnterTargetObject(new TargetObject(actual4));
            Assert.True(withName.Match(actual4));
        }

        [Fact]
        public void MatchExpression()
        {
            var match = GetSelectorVisitor("YamlMatch", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameMatch", out RunspaceContext context);
            var actual6 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual6));
            Assert.True(withName.Match(actual6));

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.False(withName.Match(actual7));
        }

        [Fact]
        public void NotMatchExpression()
        {
            var notMatch = GetSelectorVisitor("YamlNotMatch", out _);
            var actual1 = GetObject((name: "value", value: "abc"));
            var actual2 = GetObject((name: "value", value: "efg"));
            var actual3 = GetObject((name: "value", value: "hij"));
            var actual4 = GetObject((name: "value", value: 0));

            Assert.False(notMatch.Match(actual1));
            Assert.False(notMatch.Match(actual2));
            Assert.True(notMatch.Match(actual3));
            Assert.True(notMatch.Match(actual4));

            // With name
            var withName = GetSelectorVisitor("YamlNameNotMatch", out RunspaceContext context);
            var actual6 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual6));
            Assert.False(withName.Match(actual6));

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));
        }

        [Fact]
        public void InExpression()
        {
            var @in = GetSelectorVisitor("YamlIn", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameIn", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual8 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }

        [Fact]
        public void NotInExpression()
        {
            var notIn = GetSelectorVisitor("YamlNotIn", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameNotIn", out RunspaceContext context);
            var actual6 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual6));
            Assert.False(withName.Match(actual6));

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));
        }

        [Fact]
        public void SetOfExpression()
        {
            var setOf = GetSelectorVisitor("YamlSetOf", out _);
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
            var subset = GetSelectorVisitor("YamlSubset", out _);
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
            var count = GetSelectorVisitor("YamlCount", out _);
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
            var less = GetSelectorVisitor("YamlLess", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameLess", out RunspaceContext context);
            var actual8 = GetObject(
               (name: "Name", value: "ItemTwo")
            );
            var actual9 = GetObject(
               (name: "Name", value: "ItemThree")
            );

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.True(withName.Match(actual8));

            context.EnterTargetObject(new TargetObject(actual9));
            Assert.False(withName.Match(actual9));
        }

        [Fact]
        public void LessOrEqualsExpression()
        {
            var lessOrEquals = GetSelectorVisitor("YamlLessOrEquals", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameLessOrEquals", out RunspaceContext context);
            var actual8 = GetObject(
               (name: "Name", value: "ItemTwo")
            );
            var actual9 = GetObject(
               (name: "Name", value: "ItemThree")
            );

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.True(withName.Match(actual8));

            context.EnterTargetObject(new TargetObject(actual9));
            Assert.False(withName.Match(actual9));
        }

        [Fact]
        public void GreaterExpression()
        {
            var greater = GetSelectorVisitor("YamlGreater", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameGreater", out RunspaceContext context);
            var actual8 = GetObject(
               (name: "Name", value: "ItemTwo")
            );
            var actual9 = GetObject(
               (name: "Name", value: "ItemThree")
            );

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));

            context.EnterTargetObject(new TargetObject(actual9));
            Assert.True(withName.Match(actual9));
        }

        [Fact]
        public void GreaterOrEqualsExpression()
        {
            var greaterOrEquals = GetSelectorVisitor("YamlGreaterOrEquals", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameGreaterOrEquals", out RunspaceContext context);
            var actual8 = GetObject(
               (name: "Name", value: "ItemTwo")
            );
            var actual9 = GetObject(
               (name: "Name", value: "ItemThree")
            );

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));

            context.EnterTargetObject(new TargetObject(actual9));
            Assert.True(withName.Match(actual9));
        }

        [Fact]
        public void StartsWithExpression()
        {
            var startsWith = GetSelectorVisitor("YamlStartsWith", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameStartsWith", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "1TargetObject")
            );
            var actual8 = GetObject(
               (name: "Name", value: "2TargetObject")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }

        [Fact]
        public void EndsWithExpression()
        {
            var endsWith = GetSelectorVisitor("YamlEndsWith", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameEndsWith", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual8 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }

        [Fact]
        public void ContainsExpression()
        {
            var contains = GetSelectorVisitor("YamlContains", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameContains", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "Target.1.Object")
            );
            var actual8 = GetObject(
               (name: "Name", value: "Target.2.Object")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }

        [Fact]
        public void IsStringExpression()
        {
            var isStringTrue = GetSelectorVisitor("YamlIsStringTrue", out _);
            var isStringFalse = GetSelectorVisitor("YamlIsStringFalse", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameIsString", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "TargetObject1")
            );
            var actual8 = GetObject(
               (name: "Name", value: 1)
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.True(withName.Match(actual8));
        }

        [Fact]
        public void IsLowerExpression()
        {
            var isLowerTrue = GetSelectorVisitor("YamlIsLowerTrue", out _);
            var isLowerFalse = GetSelectorVisitor("YamlIsLowerFalse", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameIsLower", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "targetobject1")
            );
            var actual8 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }

        [Fact]
        public void IsUpperExpression()
        {
            var isUpperTrue = GetSelectorVisitor("YamlIsUpperTrue", out _);
            var isUpperFalse = GetSelectorVisitor("YamlIsUpperFalse", out _);
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

            // With name
            var withName = GetSelectorVisitor("YamlNameIsUpper", out RunspaceContext context);
            var actual7 = GetObject(
               (name: "Name", value: "TARGETOBJECT1")
            );
            var actual8 = GetObject(
               (name: "Name", value: "TargetObject2")
            );

            context.EnterTargetObject(new TargetObject(actual7));
            Assert.True(withName.Match(actual7));

            context.EnterTargetObject(new TargetObject(actual8));
            Assert.False(withName.Match(actual8));
        }


        [Fact]
        public void HasSchemaExpression()
        {
            var hasSchema = GetSelectorVisitor("YamlHasSchema", out _);
            var actual1 = GetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"));
            var actual2 = GetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json"));
            var actual3 = GetObject((name: "key", value: "value"), (name: "$schema", value: "http://schema.management.azure.com/schemas/2019-04-01/DeploymentParameters.json#"));
            var actual4 = GetObject((name: "key", value: "value"), (name: "$schema", value: null));
            var actual5 = GetObject((name: "key", value: "value"), (name: "$schema", value: ""));
            var actual6 = GetObject();

            Assert.True(hasSchema.Match(actual1));
            Assert.True(hasSchema.Match(actual2));
            Assert.False(hasSchema.Match(actual3));
            Assert.False(hasSchema.Match(actual4));
            Assert.False(hasSchema.Match(actual5));
            Assert.False(hasSchema.Match(actual6));

            hasSchema = GetSelectorVisitor("YamlHasSchemaIgnoreScheme", out _);
            Assert.True(hasSchema.Match(actual1));
            Assert.True(hasSchema.Match(actual2));
            Assert.True(hasSchema.Match(actual3));
            Assert.False(hasSchema.Match(actual4));
            Assert.False(hasSchema.Match(actual5));
            Assert.False(hasSchema.Match(actual6));

            hasSchema = GetSelectorVisitor("YamlHasSchemaCaseSensitive", out _);
            Assert.True(hasSchema.Match(actual1));
            Assert.True(hasSchema.Match(actual2));
            Assert.False(hasSchema.Match(actual3));
            Assert.False(hasSchema.Match(actual4));
            Assert.False(hasSchema.Match(actual5));
            Assert.False(hasSchema.Match(actual6));

            hasSchema = GetSelectorVisitor("YamlHasAnySchema", out _);
            Assert.True(hasSchema.Match(actual1));
            Assert.True(hasSchema.Match(actual2));
            Assert.True(hasSchema.Match(actual3));
            Assert.False(hasSchema.Match(actual4));
            Assert.False(hasSchema.Match(actual5));
            Assert.False(hasSchema.Match(actual6));
        }

        [Fact]
        public void Version()
        {
            var actual1 = GetObject((name: "version", value: "1.2.3"));
            var actual2 = GetObject((name: "version", value: "0.2.3"));
            var actual3 = GetObject((name: "version", value: "2.2.3"));
            var actual4 = GetObject((name: "version", value: "1.1.3"));
            var actual5 = GetObject((name: "version", value: "1.3.3-preview.1"));
            var actual6 = GetObject();
            var actual7 = GetObject((name: "version", value: "a.b.c"));

            var version = GetSelectorVisitor("YamlVersion", out _);
            Assert.True(version.Match(actual1));
            Assert.False(version.Match(actual2));
            Assert.False(version.Match(actual3));
            Assert.False(version.Match(actual4));
            Assert.False(version.Match(actual5));
            Assert.False(version.Match(actual6));
            Assert.False(version.Match(actual7));

            version = GetSelectorVisitor("YamlVersionWithPrerelease", out _);
            Assert.True(version.Match(actual1));
            Assert.False(version.Match(actual2));
            Assert.False(version.Match(actual3));
            Assert.False(version.Match(actual4));
            Assert.True(version.Match(actual5));
            Assert.False(version.Match(actual6));
            Assert.False(version.Match(actual7));

            version = GetSelectorVisitor("YamlVersionAnyVersion", out _);
            Assert.True(version.Match(actual1));
            Assert.True(version.Match(actual2));
            Assert.True(version.Match(actual3));
            Assert.True(version.Match(actual4));
            Assert.True(version.Match(actual5));
            Assert.False(version.Match(actual6));
            Assert.False(version.Match(actual7));
        }

        [Fact]
        public void HasDefault()
        {
            var actual1 = GetObject((name: "integerValue", value: 100), (name: "boolValue", value: true), (name: "stringValue", value: "testValue"));
            var actual2 = GetObject((name: "integerValue", value: 1));
            var actual3 = GetObject((name: "boolValue", value: false));
            var actual4 = GetObject((name: "stringValue", value: "TestValue"));
            var actual5 = GetObject();
            var actual6 = GetObject((name: "integerValue", value: new JValue(100)));
            var actual7 = GetObject((name: "boolValue", value: new JValue(true)));
            var actual8 = GetObject((name: "stringValue", value: new JValue("testValue")));

            var hasDefault = GetSelectorVisitor("YamlHasDefault", out _);
            Assert.True(hasDefault.Match(actual1));
            Assert.False(hasDefault.Match(actual2));
            Assert.False(hasDefault.Match(actual3));
            Assert.False(hasDefault.Match(actual4));
            Assert.True(hasDefault.Match(actual5));
            Assert.True(hasDefault.Match(actual6));
            Assert.True(hasDefault.Match(actual7));
            Assert.True(hasDefault.Match(actual8));
        }

        #endregion Conditions

        #region Operators

        [Fact]
        public void AllOf()
        {
            var allOf = GetSelectorVisitor("YamlAllOf", out _);
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
            var allOf = GetSelectorVisitor("YamlAnyOf", out _);
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
            var allOf = GetSelectorVisitor("YamlNot", out _);
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

        #region Properties

        [Fact]
        public void Type()
        {
            var equals = GetSelectorVisitor("YamlTypeEquals", out RunspaceContext context);
            var actual1 = GetObject();
            actual1.TypeNames.Insert(0, "CustomType1");

            context.EnterTargetObject(new TargetObject(actual1));

            Assert.True(equals.Match(actual1));
        }

        [Fact]
        public void Name()
        {
            var equals = GetSelectorVisitor("YamlNameEquals", out RunspaceContext context);
            var actual1 = GetObject(
                (name: "Name", value: "TargetObject1")
            );

            context.EnterTargetObject(new TargetObject(actual1));

            Assert.True(equals.Match(actual1));
        }

        #endregion Properties

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
                result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

            return result;
        }

        private static SelectorVisitor GetSelectorVisitor(string name, out RunspaceContext context)
        {
            context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContext(), null), null);
            context.Init(GetSource());
            context.Begin();
            var selector = HostHelper.GetSelectorYaml(GetSource(), context).ToArray();
            return new SelectorVisitor(null, name, selector.FirstOrDefault(s => s.Name == name).Spec.If);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
