// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.IO;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    [Trait(LANGUAGE, LANGUAGEELEMENT)]
    public sealed class AssertTests
    {
        private const string LANGUAGE = "Language";
        private const string LANGUAGEELEMENT = "Variable";

        [Fact]
        public void Assertion()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var actual1 = assert.Create(false, "Test reason");
            var actual2 = assert.Create(true, "Test reason");
            Assert.Equal("Test reason", actual1.ToString());
            Assert.False(actual1.Result);
            Assert.Equal(string.Empty, actual2.ToString());
            Assert.True(actual2.Result);

            // WithReason
            actual1.WithReason("Alternate reason");
            Assert.Equal("Test reason Alternate reason", actual1.ToString());
            actual1.WithReason("Alternate reason", true);
            Assert.Equal("Alternate reason", actual1.ToString());

            // Reason
            actual1.Reason("New {0}", "Reason");
            actual1.Reason("New New Reason");
            Assert.Equal("New New Reason", actual1.ToString());

            var actual3 = assert.Fail("Fail reason");
            Assert.Equal("Fail reason", actual3.ToString());
            actual3 = assert.Fail("Fail {0}", "reason");
            Assert.Equal("Fail reason", actual3.ToString());
        }

        [Fact]
        public void WithinRollupBlock()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var actual1 = PSRule.Runtime.RuleConditionHelper.Create(new object[] { PSObject.AsPSObject(assert.Create(true, "Test reason")), PSObject.AsPSObject(assert.Create(false, "Test reason")) });
            Assert.True(actual1.AnyOf());
            Assert.False(actual1.AllOf());

            var actual2 = PSRule.Runtime.RuleConditionHelper.Create(new object[] { assert.Create(true, "Test reason"), assert.Create(false, "Test reason") });
            Assert.True(actual2.AnyOf());
            Assert.False(actual2.AllOf());
        }

        [Fact]
        public void HasJsonSchema()
        {
            SetContext();
            var assert = GetAssertionHelper();

            var actual1 = GetObject((name: "$schema", value: "abc"));
            var actual2 = GetObject((name: "schema", value: "abc"));

            Assert.True(assert.HasJsonSchema(actual1, null).Result);
            Assert.True(assert.HasJsonSchema(actual1, new string[] { "abc" }).Result);
            Assert.False(assert.HasJsonSchema(actual2, new string[] { "abc" }).Result);
            Assert.True(assert.HasJsonSchema(actual1, new string[] { "efg", "abc" }).Result);
        }

        [Fact]
        public void StartsWith()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }).Result);
            Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
            Assert.True(assert.StartsWith(value, "name", new string[] { "ABC" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "fg" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void EndsWith()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }).Result);
            Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }, caseSensitive: true).Result);
            Assert.True(assert.EndsWith(value, "name", new string[] { "EFG" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "ab" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "EFG" }, caseSensitive: true).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void Contains()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "ABC" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "fg" }).Result);
            Assert.False(assert.Contains(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.Contains(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void Version()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject(
                (name: "version", value: "1.2.3"),
                (name: "version2", value: "1.2.3-alpha.7"),
                (name: "version3", value: "3.4.5-alpha.9"),
                (name: "notversion", value: "x.y.z")
            );

            Assert.True(assert.Version(value, "version", "1.2.3").Result);
            Assert.False(assert.Version(value, "version", "0.2.3").Result);
            Assert.False(assert.Version(value, "version", "2.2.3").Result);
            Assert.False(assert.Version(value, "version", "1.1.3").Result);
            Assert.False(assert.Version(value, "version", "1.3.3").Result);
            Assert.False(assert.Version(value, "version", "1.2.2").Result);
            Assert.False(assert.Version(value, "version", "1.2.4").Result);

            Assert.True(assert.Version(value, "version", "v1.2.3").Result);
            Assert.True(assert.Version(value, "version", "V1.2.3").Result);
            Assert.True(assert.Version(value, "version", "=1.2.3").Result);
            Assert.False(assert.Version(value, "version", "=0.2.3").Result);
            Assert.False(assert.Version(value, "version", "=2.2.3").Result);
            Assert.False(assert.Version(value, "version", "=1.1.3").Result);
            Assert.False(assert.Version(value, "version", "=1.3.3").Result);
            Assert.False(assert.Version(value, "version", "=1.2.2").Result);
            Assert.False(assert.Version(value, "version", "=1.2.4").Result);

            Assert.True(assert.Version(value, "version", "^1.2.3").Result);
            Assert.False(assert.Version(value, "version", "^0.2.3").Result);
            Assert.False(assert.Version(value, "version", "^2.2.3").Result);
            Assert.True(assert.Version(value, "version", "^1.1.3").Result);
            Assert.False(assert.Version(value, "version", "^1.3.3").Result);
            Assert.True(assert.Version(value, "version", "^1.2.2").Result);
            Assert.False(assert.Version(value, "version", "^1.2.4").Result);

            Assert.True(assert.Version(value, "version", "~1.2.3").Result);
            Assert.False(assert.Version(value, "version", "~0.2.3").Result);
            Assert.False(assert.Version(value, "version", "~2.2.3").Result);
            Assert.False(assert.Version(value, "version", "~1.1.3").Result);
            Assert.False(assert.Version(value, "version", "~1.3.3").Result);
            Assert.True(assert.Version(value, "version", "~1.2.2").Result);
            Assert.False(assert.Version(value, "version", "~1.2.4").Result);

            Assert.True(assert.Version(value, "version", "1.x").Result);
            Assert.True(assert.Version(value, "version", "1.X.x").Result);
            Assert.True(assert.Version(value, "version", "1.*").Result);
            Assert.True(assert.Version(value, "version", "*").Result);
            Assert.True(assert.Version(value, "version", "").Result);
            Assert.True(assert.Version(value, "version").Result);
            Assert.False(assert.Version(value, "version", "1.3.x").Result);

            Assert.False(assert.Version(value, "version", ">1.2.3").Result);
            Assert.True(assert.Version(value, "version", ">0.2.3").Result);
            Assert.False(assert.Version(value, "version", ">2.2.3").Result);
            Assert.True(assert.Version(value, "version", ">1.1.3").Result);
            Assert.False(assert.Version(value, "version", ">1.3.3").Result);
            Assert.True(assert.Version(value, "version", ">1.2.2").Result);
            Assert.False(assert.Version(value, "version", ">1.2.4").Result);

            Assert.True(assert.Version(value, "version", ">=1.2.3").Result);
            Assert.True(assert.Version(value, "version", ">=0.2.3").Result);
            Assert.False(assert.Version(value, "version", ">=2.2.3").Result);
            Assert.True(assert.Version(value, "version", ">=1.1.3").Result);
            Assert.False(assert.Version(value, "version", ">=1.3.3").Result);
            Assert.True(assert.Version(value, "version", ">=1.2.2").Result);
            Assert.False(assert.Version(value, "version", ">=1.2.4").Result);

            Assert.False(assert.Version(value, "version", "<1.2.3").Result);
            Assert.False(assert.Version(value, "version", "<0.2.3").Result);
            Assert.True(assert.Version(value, "version", "<2.2.3").Result);
            Assert.False(assert.Version(value, "version", "<1.1.3").Result);
            Assert.True(assert.Version(value, "version", "<1.3.3").Result);
            Assert.False(assert.Version(value, "version", "<1.2.2").Result);
            Assert.True(assert.Version(value, "version", "<1.2.4").Result);

            Assert.True(assert.Version(value, "version", "<=1.2.3").Result);
            Assert.False(assert.Version(value, "version", "<=0.2.3").Result);
            Assert.True(assert.Version(value, "version", "<=2.2.3").Result);
            Assert.False(assert.Version(value, "version", "<=1.1.3").Result);
            Assert.True(assert.Version(value, "version", "<=1.3.3").Result);
            Assert.False(assert.Version(value, "version", "<=1.2.2").Result);
            Assert.True(assert.Version(value, "version", "<=1.2.4").Result);

            Assert.True(assert.Version(value, "version", ">1.0.0").Result);
            Assert.True(assert.Version(value, "version", "<2.0.0").Result);

            Assert.True(assert.Version(value, "version", ">1.2.3-alpha.3").Result);
            Assert.True(assert.Version(value, "version2", ">1.2.3-alpha.3").Result);
            Assert.False(assert.Version(value, "version3", ">1.2.3-alpha.3").Result);

            Assert.False(assert.Version(value, "notversion", null).Result);
            Assert.Throws<RuleRuntimeException>(() => assert.Version(value, "version", "2.0.0<").Result);
            Assert.Throws<RuleRuntimeException>(() => assert.Version(value, "version", "z2.0.0").Result);
        }

        [Fact]
        public void Greater()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "value", value: 3));

            // Int
            Assert.True(assert.Greater(value, "value", 2).Result);
            Assert.False(assert.Greater(value, "value", 3).Result);
            Assert.False(assert.Greater(value, "value", 4).Result);
            Assert.True(assert.Greater(value, "value", 0).Result);
            Assert.True(assert.Greater(value, "value", -1).Result);

            // String
            value = GetObject((name: "value", value: "abc"));
            Assert.True(assert.Greater(value, "value", 2).Result);
            Assert.False(assert.Greater(value, "value", 3).Result);
            Assert.False(assert.Greater(value, "value", 4).Result);
            Assert.True(assert.Greater(value, "value", 0).Result);
            Assert.True(assert.Greater(value, "value", -1).Result);

            // Array
            value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
            Assert.True(assert.Greater(value, "value", 2).Result);
            Assert.False(assert.Greater(value, "value", 3).Result);
            Assert.False(assert.Greater(value, "value", 4).Result);
            Assert.True(assert.Greater(value, "value", 0).Result);
            Assert.True(assert.Greater(value, "value", -1).Result);

            // Self
            Assert.True(assert.Greater(3, ".", 2).Result);
        }

        [Fact]
        public void GreaterOrEqual()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "value", value: 3));

            // Int
            Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
            Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

            // String
            value = GetObject((name: "value", value: "abc"));
            Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
            Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

            // Array
            value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
            Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
            Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
            Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

            // Self
            Assert.True(assert.GreaterOrEqual(2, ".", 2).Result);
        }

        [Fact]
        public void Less()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "value", value: 3));

            // Int
            Assert.False(assert.Less(value, "value", 2).Result);
            Assert.False(assert.Less(value, "value", 3).Result);
            Assert.True(assert.Less(value, "value", 4).Result);
            Assert.False(assert.Less(value, "value", 0).Result);
            Assert.False(assert.Less(value, "value", -1).Result);

            // String
            value = GetObject((name: "value", value: "abc"));
            Assert.False(assert.Less(value, "value", 2).Result);
            Assert.False(assert.Less(value, "value", 3).Result);
            Assert.True(assert.Less(value, "value", 4).Result);
            Assert.False(assert.Less(value, "value", 0).Result);
            Assert.False(assert.Less(value, "value", -1).Result);

            // Array
            value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
            Assert.False(assert.Less(value, "value", 2).Result);
            Assert.False(assert.Less(value, "value", 3).Result);
            Assert.True(assert.Less(value, "value", 4).Result);
            Assert.False(assert.Less(value, "value", 0).Result);
            Assert.False(assert.Less(value, "value", -1).Result);

            // Self
            Assert.True(assert.Less(1, ".", 2).Result);
        }

        [Fact]
        public void LessOrEqual()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "value", value: 3));

            // Int
            Assert.False(assert.LessOrEqual(value, "value", 2).Result);
            Assert.True(assert.LessOrEqual(value, "value", 3).Result);
            Assert.True(assert.LessOrEqual(value, "value", 4).Result);
            Assert.False(assert.LessOrEqual(value, "value", 0).Result);
            Assert.False(assert.LessOrEqual(value, "value", -1).Result);

            // String
            value = GetObject((name: "value", value: "abc"));
            Assert.False(assert.LessOrEqual(value, "value", 2).Result);
            Assert.True(assert.LessOrEqual(value, "value", 3).Result);
            Assert.True(assert.LessOrEqual(value, "value", 4).Result);
            Assert.False(assert.LessOrEqual(value, "value", 0).Result);
            Assert.False(assert.LessOrEqual(value, "value", -1).Result);

            // Array
            value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
            Assert.False(assert.LessOrEqual(value, "value", 2).Result);
            Assert.True(assert.LessOrEqual(value, "value", 3).Result);
            Assert.True(assert.LessOrEqual(value, "value", 4).Result);
            Assert.False(assert.LessOrEqual(value, "value", 0).Result);
            Assert.False(assert.LessOrEqual(value, "value", -1).Result);

            // Self
            Assert.True(assert.LessOrEqual(1, ".", 1).Result);
        }

        [Fact]
        public void In()
        {
            SetContext();
            var assert = GetAssertionHelper();

            // Int
            var value = GetObject((name: "value", value: 3), (name: "values", value: new int[] { 3, 5 }));
            Assert.True(assert.In(value, "value", new int[] { 3 }).Result);
            Assert.True(assert.In(value, "value", new int[] { 2, 3, 5 }).Result);
            Assert.False(assert.In(value, "value", new int[] { 4 }).Result);
            Assert.False(assert.In(value, "value", new int[] { 2, 4, 5 }).Result);

            Assert.True(assert.In(value, "values", new int[] { 3 }).Result);
            Assert.True(assert.In(value, "values", new int[] { 2, 3, 5 }).Result);
            Assert.False(assert.In(value, "values", new int[] { 4 }).Result);
            Assert.False(assert.In(value, "values", new int[] { 4, 2 }).Result);
            Assert.True(assert.In(value, "values", new int[] { 2, 4, 5 }).Result);

            // Float
            value = GetObject((name: "value", value: 3.0f), (name: "values", value: new float[] { 3f, 5f }));
            Assert.True(assert.In(value, "value", new float[] { 3.0f }).Result);
            Assert.True(assert.In(value, "value", new float[] { 2f, 3.0f, 5f }).Result);
            Assert.False(assert.In(value, "value", new float[] { 4f }).Result);
            Assert.False(assert.In(value, "value", new float[] { 2f, 4f, 5f }).Result);

            Assert.True(assert.In(value, "values", new float[] { 3.0f }).Result);
            Assert.True(assert.In(value, "values", new float[] { 2f, 3.0f, 5f }).Result);
            Assert.False(assert.In(value, "values", new float[] { 4f }).Result);
            Assert.False(assert.In(value, "values", new float[] { 4f, 2f }).Result);
            Assert.True(assert.In(value, "values", new float[] { 2f, 4f, 5f }).Result);

            // String
            value = GetObject((name: "value", value: "value2"), (name: "values", value: new string[] { "value2", "value5" }));
            Assert.True(assert.In(value, "value", new string[] { "Value2" }).Result);
            Assert.True(assert.In(value, "value", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
            Assert.False(assert.In(value, "value", new string[] { "Value3" }).Result);
            Assert.False(assert.In(value, "value", new string[] { "VALUE1", "VALUE3" }).Result);
            Assert.False(assert.In(value, "value", new string[] { "Value2" }, true).Result);
            Assert.False(assert.In(value, "value", new string[] { "VALUE1", "VALUE2", "VALUE3" }, true).Result);
            Assert.True(assert.In(value, "value", new string[] { "value2" }, true).Result);
            Assert.True(assert.In(value, "value", new string[] { "value1", "value2", "value3" }, true).Result);

            Assert.True(assert.In(value, "values", new string[] { "Value2" }).Result);
            Assert.True(assert.In(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
            Assert.True(assert.In(value, "values", new string[] { "Value3", "Value5" }).Result);
            Assert.False(assert.In(value, "values", new string[] { "Value1", "Value3" }).Result);
            Assert.False(assert.In(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }, true).Result);
        }

        [Fact]
        public void NotIn()
        {
            SetContext();
            var assert = GetAssertionHelper();

            // Int
            var value = GetObject((name: "value", value: 3), (name: "values", value: new int[] { 3, 5 }));
            Assert.False(assert.NotIn(value, "value", new int[] { 3 }).Result);
            Assert.False(assert.NotIn(value, "value", new int[] { 2, 3, 5 }).Result);
            Assert.True(assert.NotIn(value, "value", new int[] { 4 }).Result);
            Assert.True(assert.NotIn(value, "value", new int[] { 2, 4, 5 }).Result);

            Assert.False(assert.NotIn(value, "values", new int[] { 3 }).Result);
            Assert.False(assert.NotIn(value, "values", new int[] { 2, 3, 5 }).Result);
            Assert.True(assert.NotIn(value, "values", new int[] { 4 }).Result);
            Assert.True(assert.NotIn(value, "values", new int[] { 4, 2 }).Result);
            Assert.False(assert.NotIn(value, "values", new int[] { 2, 4, 5 }).Result);

            // Float
            value = GetObject((name: "value", value: 3.0f), (name: "values", value: new float[] { 3f, 5f }));
            Assert.False(assert.NotIn(value, "value", new float[] { 3.0f }).Result);
            Assert.False(assert.NotIn(value, "value", new float[] { 2f, 3.0f, 5f }).Result);
            Assert.True(assert.NotIn(value, "value", new float[] { 4f }).Result);
            Assert.True(assert.NotIn(value, "value", new float[] { 2f, 4f, 5f }).Result);

            Assert.False(assert.NotIn(value, "values", new float[] { 3.0f }).Result);
            Assert.False(assert.NotIn(value, "values", new float[] { 2f, 3.0f, 5f }).Result);
            Assert.True(assert.NotIn(value, "values", new float[] { 4f }).Result);
            Assert.True(assert.NotIn(value, "values", new float[] { 4f, 2f }).Result);
            Assert.False(assert.NotIn(value, "values", new float[] { 2f, 4f, 5f }).Result);

            // String
            value = GetObject((name: "value", value: "value2"), (name: "values", value: new string[] { "value2", "value5" }));
            Assert.False(assert.NotIn(value, "value", new string[] { "Value2" }).Result);
            Assert.False(assert.NotIn(value, "value", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
            Assert.True(assert.NotIn(value, "value", new string[] { "Value3" }).Result);
            Assert.True(assert.NotIn(value, "value", new string[] { "VALUE1", "VALUE3" }).Result);
            Assert.True(assert.NotIn(value, "value", new string[] { "Value2" }, true).Result);
            Assert.True(assert.NotIn(value, "value", new string[] { "VALUE1", "VALUE2", "VALUE3" }, true).Result);
            Assert.False(assert.NotIn(value, "value", new string[] { "value2" }, true).Result);
            Assert.False(assert.NotIn(value, "value", new string[] { "value1", "value2", "value3" }, true).Result);

            Assert.False(assert.NotIn(value, "values", new string[] { "Value2" }).Result);
            Assert.False(assert.NotIn(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
            Assert.False(assert.NotIn(value, "values", new string[] { "Value3", "Value5" }).Result);
            Assert.True(assert.NotIn(value, "values", new string[] { "Value1", "Value3" }).Result);
            Assert.True(assert.NotIn(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }, true).Result);

            // Empty
            value = GetObject((name: "null", value: null), (name: "empty", value: new string[] { }));
            Assert.True(assert.NotIn(value, "null", new string[] { "Value1", "Value3" }).Result);
            Assert.True(assert.NotIn(value, "empty", new string[] { "Value1", "Value3" }).Result);
            Assert.True(assert.NotIn(value, "notValue", new string[] { "Value1", "Value3" }).Result);
        }

        [Fact]
        public void Match()
        {
            SetContext();
            var assert = GetAssertionHelper();

            var value = GetObject((name: "value", value: "Value1"));
            Assert.True(assert.Match(value, "value", "Value1").Result);
            Assert.True(assert.Match(value, "value", "Value[0-9]").Result);
            Assert.True(assert.Match(value, "value", "value1").Result);
            Assert.False(assert.Match(value, "value", "value2").Result);
            Assert.False(assert.Match(value, "value", "value1", true).Result);
            Assert.True(assert.Match(value, "value", "\\w*1").Result);
        }

        [Fact]
        public void NotMatch()
        {
            SetContext();
            var assert = GetAssertionHelper();

            var value = GetObject((name: "value", value: "Value2"));
            Assert.True(assert.NotMatch(value, "value", "Value1").Result);
            Assert.False(assert.NotMatch(value, "value", "Value[0-9]").Result);
            Assert.True(assert.NotMatch(value, "value", "value1").Result);
            Assert.False(assert.NotMatch(value, "value", "value2").Result);
            Assert.True(assert.NotMatch(value, "value", "value2", true).Result);
            Assert.False(assert.NotMatch(value, "value", "\\w*2").Result);
            Assert.True(assert.NotMatch(value, "notValue", "\\w*2").Result);
        }

        [Fact]
        public void FileHeader()
        {
            SetContext();
            var assert = GetAssertionHelper();

            var value = GetObject((name: "FullName", value: GetSourcePath("FromFile.Rule.ps1")));
            Assert.True(assert.FileHeader(value, "FullName", new string[] { "Copyright (c) Microsoft Corporation.", "Licensed under the MIT License." }).Result);
            value = GetObject((name: "FullName", value: GetSourcePath("Baseline.Rule.yaml")));
            Assert.False(assert.FileHeader(value, "FullName", new string[] { "Copyright (c) Microsoft Corporation.", "Licensed under the MIT License." }).Result);
        }

        [Fact]
        public void FilePath()
        {
            SetContext();
            var assert = GetAssertionHelper();

            var value = GetObject((name: "FullName", value: GetSourcePath("Baseline.Rule.yaml")));
            Assert.True(assert.FilePath(value, "FullName").Result);
            value = GetObject((name: "FullName", value: GetSourcePath("README.zz")));
            Assert.False(assert.FilePath(value, "FullName").Result);
        }

        #region Helper methods

        private static void SetContext()
        {
            var context = PipelineContext.New(new Configuration.PSRuleOption(), null, null, null, null);
            context.ExecutionScope = ExecutionScope.Condition;
            new RunspaceContext(context, null);
        }

        private static PSObject GetObject(params (string name, object value)[] properties)
        {
            var result = new PSObject();
            for (var i = 0; properties != null && i < properties.Length; i++)
                result.Properties.Add(new PSNoteProperty(properties[i].Item1, properties[i].Item2));

            return result;
        }

        private static Runtime.Assert GetAssertionHelper()
        {
            return new Runtime.Assert();
        }

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
