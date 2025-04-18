// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Runtime;
using Xunit.Abstractions;

namespace PSRule;

[Trait(LANGUAGE, LANGUAGE_ELEMENT)]
public sealed class AssertTests(ITestOutputHelper output) : ContextBaseTests
{
    private const string LANGUAGE = "Language";
    private const string LANGUAGE_ELEMENT = "Variable";

    private readonly ITestOutputHelper _Output = output;

    [Fact]
    public void Assertion()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var actual1 = assert.Create(false, "Test reason");
        var actual2 = assert.Create(true, "Test reason");
        var actual3 = assert.Fail("Fail reason");
        var actual4 = assert.Create("Name", false, "Test reason");
        Assert.Equal("Test reason", actual1.ToString());
        Assert.Equal("Test reason", actual1.GetReason()[0]);
        Assert.False(actual1.Result);
        Assert.Equal(string.Empty, actual2.ToString());
        Assert.Empty(actual2.GetReason());
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
        Assert.Equal("New New Reason", actual1.GetReason()[0]);
        actual1.ReasonIf(false, "Not a reason");
        Assert.Equal("New New Reason", actual1.ToString());
        Assert.Equal("New New Reason", actual1.GetReason()[0]);
        actual1.ReasonIf("valueX", true, "New New New Reason");
        Assert.Equal("Path valueX: New New New Reason", actual1.ToString());
        Assert.Equal("Path valueX: New New New Reason", actual1.GetReason()[0]);

        Assert.Equal("Fail reason", actual3.ToString());
        Assert.Equal("Fail reason", actual3.GetReason()[0]);
        actual3 = assert.Fail("Fail {0}", "reason");
        Assert.Equal("Fail reason", actual3.ToString());
        Assert.Equal("Fail reason", actual3.GetReason()[0]);

        var actual4Reasons = actual4.ToResultReason();
        Assert.Equal("Name", actual4Reasons[0].Path);
        Assert.Equal("Test reason", actual4Reasons[0].Message);
        Assert.Equal("Path Name: Test reason", actual4Reasons[0].Format());

        // ReasonFrom
        actual1.ReasonFrom("value", "New {0}", "Reason");
        actual1.ReasonFrom("value2", "New New Reason");
        Assert.Equal("Path value2: New New Reason", actual1.ToString());
        Assert.Equal("Path value2: New New Reason", actual1.GetReason()[0]);

        // WithPathPrefix
        actual1.PathPrefix("resources[0]");
        Assert.Equal("Path resources[0].value2: New New Reason", actual1.GetReason()[0]);
        actual4.PathPrefix("resources[0]");
        Assert.Equal("Path resources[0].Name: Test reason", actual4.GetReason()[0]);
        actual4.PathPrefix(".");
        Assert.Equal("Path Name: Test reason", actual4.GetReason()[0]);

        // Aggregate results
        Assert.True(assert.AnyOf(actual2, actual3).Result);
        Assert.True(assert.AnyOf(actual2).Result);
        Assert.True(assert.AnyOf(actual3, actual2, actual1).Result);
        Assert.False(assert.AnyOf().Result);

        Assert.False(assert.AllOf(actual2, actual3).Result);
        var test1 = assert.AllOf(actual2, actual3);
        Assert.Equal("Fail reason", test1.ToString());
        Assert.Equal("Fail reason", test1.GetReason()[0]);

        var test2 = assert.AllOf(actual1, actual2, actual3);
        Assert.Equal("Path resources[0].value2: New New Reason Fail reason", test2.ToString());
        Assert.Equal(new string[] { "Path resources[0].value2: New New Reason", "Fail reason" }, test2.GetReason());
        Assert.True(assert.AllOf(actual2, actual2).Result);
        Assert.True(assert.AllOf(actual2).Result);
        Assert.False(assert.AllOf().Result);
    }

    /// <summary>
    /// Tests for <see cref="Runtime.Assert.AnyOf"/>.
    /// </summary>
    [Fact]
    public void AnyOf()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var actual1 = assert.Create(false, "Test reason 1");
        var actual2 = assert.Create(true, "Test reason 2");
        var actual3 = assert.Fail("Fail reason");

        // Pass cases with no reasons.
        var result = assert.AnyOf(actual2, actual3);
        Assert.True(result.Result);
        Assert.Empty(result.GetReason());
        Assert.True(assert.AnyOf(actual2).Result);
        Assert.True(assert.AnyOf(actual3, actual2, actual1).Result);

        // Check reasons are returned.
        result = assert.AnyOf(actual1, actual3);
        Assert.False(result.Result);
        Assert.Equal("Test reason 1 Fail reason", result.ToString());
        Assert.Equal<string>(["Test reason 1", "Fail reason"], result.GetReason());

        // Empty fail case.
        Assert.False(assert.AnyOf().Result);
    }

    /// <summary>
    /// Tests for <see cref="Runtime.Assert.AllOf"/>.
    /// </summary>
    [Fact]
    public void AllOf()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var actual1 = assert.Create(false, "Test reason 1");
        var actual2 = assert.Create(true, "Test reason 2");
        var actual3 = assert.Fail("Fail reason");

        // Fail cases.
        var result = assert.AllOf(actual2, actual3);
        Assert.False(result.Result);
        Assert.Equal("Fail reason", result.ToString());
        Assert.Equal("Fail reason", result.GetReason()[0]);

        result = assert.AllOf(actual1, actual2, actual3);
        Assert.Equal("Test reason 1 Fail reason", result.ToString());
        Assert.Equal<string>(["Test reason 1", "Fail reason"], result.GetReason());

        // Pass cases.
        Assert.True(assert.AllOf(actual2, actual2).Result);
        Assert.True(assert.AllOf(actual2).Result);

        // Empty fail case.
        Assert.False(assert.AllOf().Result);
    }

    [Fact]
    public void WithinRollupBlock()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var actual1 = RuleConditionHelper.Create([PSObject.AsPSObject(assert.Create(true, "Test reason")), PSObject.AsPSObject(assert.Create(false, "Test reason"))]);
        Assert.True(actual1.AnyOf());
        Assert.False(actual1.AllOf());

        var actual2 = RuleConditionHelper.Create([assert.Create(true, "Test reason"), assert.Create(false, "Test reason")]);
        Assert.True(actual2.AnyOf());
        Assert.False(actual2.AllOf());
    }

    [Fact]
    public void HasFieldValue()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "Value4", value: 0),
            (name: "value5", value: GetObject((name: "value", value: 0)))
        );

        Assert.False(assert.HasFieldValue(null, null).Result);
        Assert.True(assert.HasFieldValue(value, "value").Result);
        Assert.False(assert.HasFieldValue(value, "value2").Result);
        Assert.False(assert.HasFieldValue(value, "value3").Result);
        Assert.True(assert.HasFieldValue(value, "Value4").Result);
        Assert.True(assert.HasFieldValue(value, "value5").Result);
        Assert.True(assert.HasFieldValue(value, "value5.value").Result);

        Assert.Empty(assert.HasFieldValue(value, "value").ToResultReason());
        Assert.Equal("value6", assert.HasFieldValue(value, "value6").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value7.value", assert.HasFieldValue(value, "value7.value").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void HasField()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "Value4", value: 0),
            (name: "value5", value: GetObject((name: "value", value: 0)))
        );

        Assert.False(assert.HasField(null, null).Result);
        Assert.False(assert.HasField(null, Array.Empty<string>()).Result);
        Assert.True(assert.HasField(value, new string[] { "value" }).Result);
        Assert.True(assert.HasField(value, new string[] { "notValue", "Value" }).Result);
        Assert.True(assert.HasField(value, new string[] { "value2" }).Result);
        Assert.True(assert.HasField(value, new string[] { "value3" }).Result);
        Assert.False(assert.HasField(value, new string[] { "Value3" }, true).Result);
        Assert.True(assert.HasField(value, new string[] { "Value3", "Value4" }, true).Result);
        Assert.True(assert.HasField(value, new string[] { "value5" }).Result);
        Assert.True(assert.HasField(value, new string[] { "value5.value" }).Result);
        Assert.False(assert.HasField(value, new string[] { "Value5.value" }, true).Result);
        Assert.False(assert.HasField(value, new string[] { "value5.Value" }, true).Result);

        Assert.Empty(assert.HasField(value, new string[] { "value" }).ToResultReason());
        Assert.Equal("value6", assert.HasField(value, new string[] { "value6" }).ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value7.value", assert.HasField(value, new string[] { "value7.value" }).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NotHasField()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "Value4", value: 0)
        );

        Assert.False(assert.NotHasField(null, null).Result);
        Assert.False(assert.NotHasField(null, Array.Empty<string>()).Result);
        Assert.False(assert.NotHasField(value, new string[] { "value" }).Result);
        Assert.False(assert.NotHasField(value, new string[] { "notValue", "Value" }).Result);
        Assert.True(assert.NotHasField(value, new string[] { "notValue", "Value" }, true).Result);
        Assert.False(assert.NotHasField(value, new string[] { "value2" }).Result);
        Assert.False(assert.NotHasField(value, new string[] { "value3" }).Result);
        Assert.True(assert.NotHasField(value, new string[] { "Value3" }, true).Result);
        Assert.False(assert.NotHasField(value, new string[] { "Value3", "Value4" }, true).Result);

        Assert.Empty(assert.NotHasField(value, new string[] { "notValue", "Value" }, true).ToResultReason());
        Assert.Equal("value2", assert.NotHasField(value, new string[] { "value2" }).ToResultReason().FirstOrDefault().Path);
        Assert.Equal("Value4", assert.NotHasField(value, new string[] { "Value3", "Value4" }, true).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void HasJsonSchema()
    {
        SetContext();
        var assert = GetAssertionHelper();

        const string DRAFT7_HTTP = "http://json-schema.org/draft-07/schema#";
        const string DRAFT7_HTTPS = "https://json-schema.org/draft-07/schema#";

        var actual1 = GetObject((name: "$schema", value: "abc"));
        var actual2 = GetObject((name: "schema", value: "abc"));
        var actual3 = GetObject((name: "$schema", value: DRAFT7_HTTP));
        var actual4 = GetObject((name: "$schema", value: "http://json-schema.org/draft-07/schema#definition"));
        var actual5 = GetObject((name: "$schema", value: ""));

        Assert.True(assert.HasJsonSchema(actual1, null).Result);
        Assert.True(assert.HasJsonSchema(actual1, new string[] { "abc" }).Result);
        Assert.False(assert.HasJsonSchema(actual2, new string[] { "abc" }).Result);
        Assert.True(assert.HasJsonSchema(actual1, new string[] { "efg", "abc" }).Result);
        Assert.False(assert.HasJsonSchema(actual3, new string[] { "https://json-schema.org/draft-07/schema" }).Result);
        Assert.True(assert.HasJsonSchema(actual3, new string[] { DRAFT7_HTTPS }, true).Result);
        Assert.True(assert.HasJsonSchema(actual3, new string[] { DRAFT7_HTTPS, DRAFT7_HTTP }).Result);
        Assert.False(assert.HasJsonSchema(actual4, new string[] { DRAFT7_HTTPS }, true).Result);
        Assert.False(assert.HasJsonSchema(actual5, null).Result);

        Assert.Equal("$schema", assert.HasJsonSchema(actual2, new string[] { "abc" }).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void StartsWith()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }).Result);
        Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
        Assert.True(assert.StartsWith(value, "name", new string[] { "ABC" }).Result);
        Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);
        Assert.False(assert.StartsWith(value, "name", new string[] { "123", "fg" }).Result);
        Assert.False(assert.StartsWith(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.False(assert.StartsWith(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
        Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.StartsWith(value, "name", new string[] { "abcdefgh" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.False(assert.StartsWith(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void NotStartsWith()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.False(assert.NotStartsWith(value, "name", new string[] { "123", "ab" }).Result);
        Assert.False(assert.NotStartsWith(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
        Assert.False(assert.NotStartsWith(value, "name", new string[] { "ABC" }).Result);
        Assert.True(assert.NotStartsWith(value, "name", new string[] { "123", "cd" }).Result);
        Assert.True(assert.NotStartsWith(value, "name", new string[] { "123", "fg" }).Result);
        Assert.True(assert.NotStartsWith(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.True(assert.NotStartsWith(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
        Assert.True(assert.NotStartsWith(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.NotStartsWith(value, "name", new string[] { "ABC" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.True(assert.NotStartsWith(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void EndsWith()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }).Result);
        Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }, caseSensitive: true).Result);
        Assert.True(assert.EndsWith(value, "name", new string[] { "EFG" }).Result);
        Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);
        Assert.False(assert.EndsWith(value, "name", new string[] { "123", "ab" }).Result);
        Assert.False(assert.EndsWith(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.False(assert.EndsWith(value, "name", new string[] { "EFG" }, caseSensitive: true).Result);
        Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.EndsWith(value, "name", new string[] { "abcdefgh" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.False(assert.EndsWith(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void NotEndsWith()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.False(assert.NotEndsWith(value, "name", new string[] { "123", "fg" }).Result);
        Assert.False(assert.NotEndsWith(value, "name", new string[] { "123", "fg" }, caseSensitive: true).Result);
        Assert.False(assert.NotEndsWith(value, "name", new string[] { "EFG" }).Result);
        Assert.True(assert.NotEndsWith(value, "name", new string[] { "123", "cd" }).Result);
        Assert.True(assert.NotEndsWith(value, "name", new string[] { "123", "ab" }).Result);
        Assert.True(assert.NotEndsWith(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.True(assert.NotEndsWith(value, "name", new string[] { "EFG" }, caseSensitive: true).Result);
        Assert.True(assert.NotEndsWith(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.NotEndsWith(value, "name", new string[] { "EFG" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.True(assert.NotEndsWith(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void Contains()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }).Result);
        Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
        Assert.True(assert.Contains(value, "name", new string[] { "ABC" }).Result);
        Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);
        Assert.True(assert.Contains(value, "name", new string[] { "123", "fg" }).Result);
        Assert.False(assert.Contains(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.False(assert.Contains(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
        Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.Contains(value, "name", new string[] { "abcdefgh" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.False(assert.Contains(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void NotContains()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.False(assert.NotContains(value, "name", new string[] { "123", "ab" }).Result);
        Assert.False(assert.NotContains(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
        Assert.False(assert.NotContains(value, "name", new string[] { "ABC" }).Result);
        Assert.False(assert.NotContains(value, "name", new string[] { "123", "cd" }).Result);
        Assert.False(assert.NotContains(value, "name", new string[] { "123", "fg" }).Result);
        Assert.True(assert.NotContains(value, "name", new string[] { "abcdefgh" }).Result);
        Assert.True(assert.NotContains(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
        Assert.False(assert.NotContains(value, "name", new string[] { "123", "cd" }).Result);

        Assert.Equal("name", assert.NotContains(value, "name", new string[] { "ABC" }).ToResultReason().FirstOrDefault().Path);

        // Integer
        Assert.True(assert.NotContains(value, "value", new string[] { "123" }).Result);
    }

    [Fact]
    public void IsLower()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "name1", value: "abc"),
            (name: "name2", value: "aBc"),
            (name: "name3", value: "123"),
            (name: "name4", value: 123)
        );

        Assert.True(assert.IsLower(value, "name1").Result);
        Assert.False(assert.IsLower(value, "name2").Result);
        Assert.True(assert.IsLower(value, "name3").Result);
        Assert.False(assert.IsLower(value, "name3", requireLetters: true).Result);
        Assert.False(assert.IsLower(value, "name4").Result);

        Assert.Equal("name2", assert.IsLower(value, "name2").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("name3", assert.IsLower(value, "name3", requireLetters: true).ToResultReason().FirstOrDefault().Path);
        Assert.Equal("name4", assert.IsLower(value, "name4").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsUpper()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "name1", value: "ABC"),
            (name: "name2", value: "AbC"),
            (name: "name3", value: "123"),
            (name: "name4", value: 123)
        );

        Assert.True(assert.IsUpper(value, "name1").Result);
        Assert.False(assert.IsUpper(value, "name2").Result);
        Assert.True(assert.IsUpper(value, "name3").Result);
        Assert.False(assert.IsUpper(value, "name3", requireLetters: true).Result);
        Assert.False(assert.IsUpper(value, "name4").Result);

        Assert.Equal("name2", assert.IsUpper(value, "name2").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("name3", assert.IsUpper(value, "name3", requireLetters: true).ToResultReason().FirstOrDefault().Path);
        Assert.Equal("name4", assert.IsUpper(value, "name4").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsNumeric()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: 123),
            (name: "value2", value: 1.0f),
            (name: "value3", value: long.MaxValue),
            (name: "value4", value: "123"),
            (name: "value5", value: null),
            (name: "value6", value: PSObject.AsPSObject(123)),
            (name: "value7", value: byte.MaxValue),
            (name: "value8", value: double.MaxValue)
        );

        Assert.True(assert.IsNumeric(value, "value1").Result);
        Assert.True(assert.IsNumeric(value, "value2").Result);
        Assert.True(assert.IsNumeric(value, "value3").Result);
        Assert.False(assert.IsNumeric(value, "value4").Result);
        Assert.True(assert.IsNumeric(value, "value4", convert: true).Result);
        Assert.False(assert.IsNumeric(value, "value5").Result);
        Assert.True(assert.IsNumeric(value, "value6").Result);
        Assert.True(assert.IsNumeric(value, "value7").Result);
        Assert.True(assert.IsNumeric(value, "value8").Result);

        Assert.Equal("value4", assert.IsNumeric(value, "value4").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value5", assert.IsNumeric(value, "value5").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsInteger()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: 123),
            (name: "value2", value: 1.0f),
            (name: "value3", value: long.MaxValue),
            (name: "value4", value: "123"),
            (name: "value5", value: null),
            (name: "value6", value: PSObject.AsPSObject(123)),
            (name: "value7", value: byte.MaxValue)
        );

        Assert.True(assert.IsInteger(value, "value1").Result);
        Assert.False(assert.IsInteger(value, "value2").Result);
        Assert.True(assert.IsInteger(value, "value3").Result);
        Assert.False(assert.IsInteger(value, "value4").Result);
        Assert.True(assert.IsInteger(value, "value4", convert: true).Result);
        Assert.False(assert.IsInteger(value, "value5").Result);
        Assert.True(assert.IsInteger(value, "value6").Result);
        Assert.True(assert.IsInteger(value, "value7").Result);

        Assert.Equal("value2", assert.IsInteger(value, "value2").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value4", assert.IsInteger(value, "value4").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value5", assert.IsInteger(value, "value5").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsBool()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: true),
            (name: "value2", value: 1),
            (name: "value3", value: long.MaxValue),
            (name: "value4", value: "true"),
            (name: "value5", value: null),
            (name: "value6", value: PSObject.AsPSObject(true))
        );

        Assert.True(assert.IsBoolean(value, "value1").Result);
        Assert.False(assert.IsBoolean(value, "value2").Result);
        Assert.False(assert.IsBoolean(value, "value3").Result);
        Assert.False(assert.IsBoolean(value, "value4").Result);
        Assert.True(assert.IsBoolean(value, "value4", convert: true).Result);
        Assert.False(assert.IsBoolean(value, "value5").Result);
        Assert.True(assert.IsBoolean(value, "value6").Result);

        Assert.Equal("value2", assert.IsBoolean(value, "value2").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsArray()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: new string[] { "a" }),
            (name: "value2", value: new int[] { 1 }),
            (name: "value3", value: PSObject.AsPSObject(new int[] { 1 })),
            (name: "value4", value: "true"),
            (name: "value5", value: null)
        );

        Assert.True(assert.IsArray(value, "value1").Result);
        Assert.True(assert.IsArray(value, "value2").Result);
        Assert.True(assert.IsArray(value, "value3").Result);
        Assert.False(assert.IsArray(value, "value4").Result);
        Assert.False(assert.IsArray(value, "value5").Result);

        Assert.Equal("value4", assert.IsArray(value, "value4").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsString()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: "true"),
            (name: "value2", value: PSObject.AsPSObject("true")),
            (name: "value3", value: 1),
            (name: "value4", value: null)
        );

        Assert.True(assert.IsString(value, "value1").Result);
        Assert.True(assert.IsString(value, "value2").Result);
        Assert.False(assert.IsString(value, "value3").Result);
        Assert.False(assert.IsString(value, "value4").Result);

        Assert.Equal("value4", assert.IsString(value, "value4").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void IsDateTime()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "value1", value: DateTime.Now),
            (name: "value2", value: 1),
            (name: "value3", value: long.MaxValue),
            (name: "value4", value: "2021-04-03T15:00:00.00+10:00"),
            (name: "value5", value: null),
            (name: "value6", value: PSObject.AsPSObject(DateTime.Now)),
            (name: "value7", value: new JValue(DateTime.Now)),
            (name: "value8", value: new JValue("2021-04-03T15:00:00.00+10:00"))
        );

        Assert.True(assert.IsDateTime(value, "value1").Result);
        Assert.False(assert.IsDateTime(value, "value2").Result);
        Assert.False(assert.IsDateTime(value, "value3").Result);
        Assert.False(assert.IsDateTime(value, "value4").Result);
        Assert.True(assert.IsDateTime(value, "value4", convert: true).Result);
        Assert.False(assert.IsDateTime(value, "value5").Result);
        Assert.True(assert.IsDateTime(value, "value6").Result);
        Assert.True(assert.IsDateTime(value, "value7").Result);
        Assert.True(assert.IsDateTime(value, "value8", convert: true).Result);

        Assert.Equal("value2", assert.IsDateTime(value, "value2").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("value5", assert.IsDateTime(value, "value5").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void TypeOf()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var customObect = new PSObject();
        customObect.TypeNames.Insert(0, "CustomTypeObject");
        var value = GetObject(
            (name: "value1", value: "true"),
            (name: "value2", value: PSObject.AsPSObject("true")),
            (name: "value3", value: 1),
            (name: "value4", value: null),
            (name: "value5", value: customObect)
        );

        // By type
        Assert.True(assert.TypeOf(value, "value1", new Type[] { typeof(string) }).Result);
        Assert.True(assert.TypeOf(value, "value2", new Type[] { typeof(string) }).Result);
        Assert.True(assert.TypeOf(value, "value3", new Type[] { typeof(int) }).Result);
        Assert.False(assert.TypeOf(value, "value3", new Type[] { typeof(bool) }).Result);
        Assert.True(assert.TypeOf(value, "value3", new Type[] { typeof(bool), typeof(int) }).Result);
        Assert.False(assert.TypeOf(value, "value3", new Type[] { typeof(string) }).Result);
        Assert.False(assert.TypeOf(value, "value4", new Type[] { typeof(string) }).Result);

        Assert.Equal("value3", assert.TypeOf(value, "value3", new Type[] { typeof(bool) }).ToResultReason().FirstOrDefault().Path);

        // By type name
        Assert.True(assert.TypeOf(value, "value1", new string[] { "System.String" }).Result);
        Assert.True(assert.TypeOf(value, "value2", new string[] { "System.String" }).Result);
        Assert.True(assert.TypeOf(value, "value3", new string[] { "System.Int32" }).Result);
        Assert.False(assert.TypeOf(value, "value4", new string[] { "System.Int32" }).Result);
        Assert.True(assert.TypeOf(value, "value5", new string[] { "CustomTypeObject" }).Result);

        Assert.Equal("value4", assert.TypeOf(value, "value4", new string[] { "System.Int32" }).ToResultReason().FirstOrDefault().Path);
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

        Assert.True(assert.Version(value, "version", ">=1.2.3-0").Result);
        Assert.True(assert.Version(value, "version2", ">=1.2.3-0").Result);

        Assert.True(assert.Version(value, "version", ">=1.2.3", includePrerelease: true).Result);
        Assert.False(assert.Version(value, "version2", ">=1.2.3", includePrerelease: true).Result);
        Assert.True(assert.Version(value, "version3", ">=1.2.3", includePrerelease: true).Result);

        Assert.False(assert.Version(value, "notversion", null).Result);
        Assert.Throws<RuleException>(() => assert.Version(value, "version", "2.0.0<").Result);
        Assert.Throws<RuleException>(() => assert.Version(value, "version", "z2.0.0").Result);

        Assert.Equal("version", assert.Version(value, "version", "<=0.2.3").ToResultReason().FirstOrDefault().Path);
        Assert.Equal("notversion", assert.Version(value, "notversion", null).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void APIVersion()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject(
            (name: "version", value: "2015-10-01"),
            (name: "version2", value: "2015-10-01-preview"),
            (name: "version3", value: "2022-01-01-preview"),
            (name: "notversion", value: "x.y.z")
        );

        Assert.True(assert.APIVersion(value, "version", "2015-10-01").Result);
        Assert.True(assert.APIVersion(value, "version", "=2015-10-01").Result);
        Assert.True(assert.APIVersion(value, "version", ">=2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version", "<2015-10-01").Result);
        Assert.True(assert.APIVersion(value, "version", ">2014-01-01").Result);
        Assert.True(assert.APIVersion(value, "version", ">=2015-10-01-preview").Result);
        Assert.True(assert.APIVersion(value, "version", ">=2015-10-01-preview", includePrerelease: true).Result);

        Assert.False(assert.APIVersion(value, "version2", "2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version2", "=2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version2", ">=2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version2", "<2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version2", ">2014-01-01").Result);
        Assert.True(assert.APIVersion(value, "version2", ">=2015-10-01-preview").Result);
        Assert.True(assert.APIVersion(value, "version2", ">=2015-10-01-preview", includePrerelease: true).Result);

        Assert.False(assert.APIVersion(value, "version3", "2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version3", "=2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version3", ">=2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version3", "<2015-10-01").Result);
        Assert.False(assert.APIVersion(value, "version3", ">2014-01-01").Result);
        Assert.False(assert.APIVersion(value, "version3", ">=2015-10-01-preview").Result);
        Assert.True(assert.APIVersion(value, "version3", ">=2015-10-01-preview", includePrerelease: true).Result);
    }

    [Fact]
    public void Greater()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "value", value: 3), (name: "jvalue", value: new JValue(3)));

        // Int
        Assert.True(assert.Greater(value, "value", 2).Result);
        Assert.False(assert.Greater(value, "value", 3).Result);
        Assert.False(assert.Greater(value, "value", 4).Result);
        Assert.True(assert.Greater(value, "value", 0).Result);
        Assert.True(assert.Greater(value, "value", -1).Result);
        Assert.False(assert.Greater(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.Greater(value, "value", 3).ToResultReason().FirstOrDefault().Path);
        Assert.Equal("jvalue", assert.Greater(value, "jvalue", 4).ToResultReason().FirstOrDefault().Path);

        // ULong
        value = GetObject((name: "value", value: (ulong)3), (name: "jvalue", value: new JValue((ulong)3)));
        Assert.True(assert.Greater(value, "value", 2).Result);
        Assert.False(assert.Greater(value, "value", 3).Result);
        Assert.False(assert.Greater(value, "value", 4).Result);
        Assert.True(assert.Greater(value, "value", 0).Result);
        Assert.True(assert.Greater(value, "value", -1).Result);
        Assert.False(assert.Greater(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.Greater(value, "value", 3).ToResultReason().FirstOrDefault().Path);

        // String
        value = GetObject((name: "value", value: "abc"));
        Assert.True(assert.Greater(value, "value", 2).Result);
        Assert.False(assert.Greater(value, "value", 3).Result);
        Assert.False(assert.Greater(value, "value", 4).Result);
        Assert.True(assert.Greater(value, "value", 0).Result);
        Assert.True(assert.Greater(value, "value", -1).Result);

        Assert.Equal("value", assert.Greater(value, "value", 3).ToResultReason().FirstOrDefault().Path);

        // Array
        value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.True(assert.Greater(value, "value", 2).Result);
        Assert.False(assert.Greater(value, "value", 3).Result);
        Assert.False(assert.Greater(value, "value", 4).Result);
        Assert.True(assert.Greater(value, "value", 0).Result);
        Assert.True(assert.Greater(value, "value", -1).Result);

        // DateTime
        value = GetObject((name: "value", value: DateTime.Now.AddDays(4)));
        Assert.True(assert.Greater(value, "value", 2).Result);
        Assert.True(assert.Greater(value, "value", 3).Result);
        Assert.False(assert.Greater(value, "value", 5).Result);
        Assert.True(assert.Greater(value, "value", 0).Result);
        Assert.True(assert.Greater(value, "value", -1).Result);

        // Self
        Assert.True(assert.Greater(3, ".", 2).Result);

        // Convert from string
        value = GetObject((name: "value", value: "3"));
        Assert.True(assert.Greater(value, "value", 2, convert: true).Result);
        Assert.False(assert.Greater(value, "value", 2, convert: false).Result);
        value = GetObject((name: "value", value: (4.5).ToString()));
        Assert.True(assert.Greater(value, "value", 2, convert: true).Result);
        Assert.False(assert.Greater(value, "value", 4, convert: false).Result);

        Assert.Equal("value", assert.Greater(value, "value", 4, convert: false).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void GreaterOrEqual()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "value", value: 3), (name: "jvalue", value: new JValue(3)));

        // Int
        Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);
        Assert.False(assert.GreaterOrEqual(value, "jvalue", 4).Result);

        Assert.Equal("jvalue", assert.GreaterOrEqual(value, "jvalue", 4).ToResultReason().FirstOrDefault().Path);

        // ULong
        value = GetObject((name: "value", value: (ulong)3), (name: "jvalue", value: new JValue((ulong)3)));
        Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);
        Assert.False(assert.GreaterOrEqual(value, "jvalue", 4).Result);

        Assert.Equal("jvalue", assert.GreaterOrEqual(value, "jvalue", 4).ToResultReason().FirstOrDefault().Path);

        // String
        value = GetObject((name: "value", value: "abc"));
        Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

        Assert.Equal("value", assert.GreaterOrEqual(value, "value", 4).ToResultReason().FirstOrDefault().Path);

        // Array
        value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 4).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

        // DateTime
        value = GetObject((name: "value", value: DateTime.Now.AddDays(4)));
        Assert.True(assert.GreaterOrEqual(value, "value", 2).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 3).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 5).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", 0).Result);
        Assert.True(assert.GreaterOrEqual(value, "value", -1).Result);

        // Self
        Assert.True(assert.GreaterOrEqual(2, ".", 2).Result);

        // Convert from string
        value = GetObject((name: "value", value: "3"));
        Assert.True(assert.GreaterOrEqual(value, "value", 2, convert: true).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 2, convert: false).Result);
        value = GetObject((name: "value", value: (4.5).ToString()));
        Assert.True(assert.GreaterOrEqual(value, "value", 2, convert: true).Result);
        Assert.False(assert.GreaterOrEqual(value, "value", 4, convert: false).Result);

        Assert.Equal("value", assert.GreaterOrEqual(value, "value", 4, convert: false).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void Less()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "value", value: 3), (name: "jvalue", value: new JValue(3)));

        // Int
        Assert.False(assert.Less(value, "value", 2).Result);
        Assert.False(assert.Less(value, "value", 3).Result);
        Assert.True(assert.Less(value, "value", 4).Result);
        Assert.False(assert.Less(value, "value", 0).Result);
        Assert.False(assert.Less(value, "value", -1).Result);
        Assert.True(assert.Less(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.Less(value, "value", -1).ToResultReason().FirstOrDefault().Path);

        // ULong
        value = GetObject((name: "value", value: (ulong)3), (name: "jvalue", value: new JValue((ulong)3)));
        Assert.False(assert.Less(value, "value", 2).Result);
        Assert.False(assert.Less(value, "value", 3).Result);
        Assert.True(assert.Less(value, "value", 4).Result);
        Assert.False(assert.Less(value, "value", 0).Result);
        Assert.False(assert.Less(value, "value", -1).Result);
        Assert.True(assert.Less(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.Less(value, "value", -1).ToResultReason().FirstOrDefault().Path);

        // String
        value = GetObject((name: "value", value: "abc"));
        Assert.False(assert.Less(value, "value", 2).Result);
        Assert.False(assert.Less(value, "value", 3).Result);
        Assert.True(assert.Less(value, "value", 4).Result);
        Assert.False(assert.Less(value, "value", 0).Result);
        Assert.False(assert.Less(value, "value", -1).Result);

        Assert.Equal("value", assert.Less(value, "value", -1).ToResultReason().FirstOrDefault().Path);

        // Array
        value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.False(assert.Less(value, "value", 2).Result);
        Assert.False(assert.Less(value, "value", 3).Result);
        Assert.True(assert.Less(value, "value", 4).Result);
        Assert.False(assert.Less(value, "value", 0).Result);
        Assert.False(assert.Less(value, "value", -1).Result);

        // DateTime
        value = GetObject((name: "value", value: DateTime.Now.AddDays(4)));
        Assert.False(assert.Less(value, "value", 2).Result);
        Assert.False(assert.Less(value, "value", 3).Result);
        Assert.True(assert.Less(value, "value", 5).Result);
        Assert.False(assert.Less(value, "value", 0).Result);
        Assert.False(assert.Less(value, "value", -1).Result);

        // Self
        Assert.True(assert.Less(1, ".", 2).Result);

        // Convert from string
        value = GetObject((name: "value", value: "3"));
        Assert.False(assert.Less(value, "value", 2, convert: true).Result);
        Assert.True(assert.Less(value, "value", 2, convert: false).Result);
        value = GetObject((name: "value", value: (4.5).ToString()));

        Assert.False(assert.Less(value, "value", 4, convert: true).Result);
        Assert.True(assert.Less(value, "value", 4, convert: false).Result);

        Assert.Equal("value", assert.Less(value, "value", 4, convert: true).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void LessOrEqual()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "value", value: 3), (name: "jvalue", value: new JValue(3)));

        // Int
        Assert.False(assert.LessOrEqual(value, "value", 2).Result);
        Assert.True(assert.LessOrEqual(value, "value", 3).Result);
        Assert.True(assert.LessOrEqual(value, "value", 4).Result);
        Assert.False(assert.LessOrEqual(value, "value", 0).Result);
        Assert.False(assert.LessOrEqual(value, "value", -1).Result);
        Assert.True(assert.LessOrEqual(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.LessOrEqual(value, "value", 0).ToResultReason().FirstOrDefault().Path);

        // ULong
        value = GetObject((name: "value", value: (ulong)3), (name: "jvalue", value: new JValue((ulong)3)));
        Assert.False(assert.LessOrEqual(value, "value", 2).Result);
        Assert.True(assert.LessOrEqual(value, "value", 3).Result);
        Assert.True(assert.LessOrEqual(value, "value", 4).Result);
        Assert.False(assert.LessOrEqual(value, "value", 0).Result);
        Assert.False(assert.LessOrEqual(value, "value", -1).Result);
        Assert.True(assert.LessOrEqual(value, "jvalue", 4).Result);

        Assert.Equal("value", assert.LessOrEqual(value, "value", 0).ToResultReason().FirstOrDefault().Path);

        // String
        value = GetObject((name: "value", value: "abc"));
        Assert.False(assert.LessOrEqual(value, "value", 2).Result);
        Assert.True(assert.LessOrEqual(value, "value", 3).Result);
        Assert.True(assert.LessOrEqual(value, "value", 4).Result);
        Assert.False(assert.LessOrEqual(value, "value", 0).Result);
        Assert.False(assert.LessOrEqual(value, "value", -1).Result);

        Assert.Equal("value", assert.LessOrEqual(value, "value", 0).ToResultReason().FirstOrDefault().Path);

        // Array
        value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.False(assert.LessOrEqual(value, "value", 2).Result);
        Assert.True(assert.LessOrEqual(value, "value", 3).Result);
        Assert.True(assert.LessOrEqual(value, "value", 4).Result);
        Assert.False(assert.LessOrEqual(value, "value", 0).Result);
        Assert.False(assert.LessOrEqual(value, "value", -1).Result);

        // DateTime
        value = GetObject((name: "value", value: DateTime.Now.AddDays(4)));
        Assert.False(assert.LessOrEqual(value, "value", 2).Result);
        Assert.False(assert.LessOrEqual(value, "value", 3).Result);
        Assert.True(assert.LessOrEqual(value, "value", 5).Result);
        Assert.False(assert.LessOrEqual(value, "value", 0).Result);
        Assert.False(assert.LessOrEqual(value, "value", -1).Result);

        // Self
        Assert.True(assert.LessOrEqual(1, ".", 1).Result);

        // Convert from string
        value = GetObject((name: "value", value: "3"));
        Assert.False(assert.LessOrEqual(value, "value", 2, convert: true).Result);
        Assert.True(assert.LessOrEqual(value, "value", 2, convert: false).Result);
        value = GetObject((name: "value", value: (4.5).ToString()));
        Assert.False(assert.LessOrEqual(value, "value", 4, convert: true).Result);
        Assert.True(assert.LessOrEqual(value, "value", 4, convert: false).Result);

        Assert.Equal("value", assert.LessOrEqual(value, "value", 4, convert: true).ToResultReason().FirstOrDefault().Path);
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
        value = GetObject((name: "value", value: "value2"), (name: "values", value: new string[] { "value2", "value5" }), (name: "objects", value: new object[] { "value2", "value5" }));
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
        Assert.True(assert.In(value, "values", new object[] { "Value3", "Value5" }).Result);
        Assert.False(assert.In(value, "values", new string[] { "Value1", "Value3" }).Result);
        Assert.False(assert.In(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }, true).Result);

        Assert.True(assert.In(value, "objects", new object[] { "Value3", "Value5" }).Result);
        Assert.True(assert.In(value, "objects", new string[] { "Value3", "Value5" }).Result);
        Assert.True(assert.In(PSObject.AsPSObject(new object[] { "value2", "value5" }), ".", new string[] { "Value3", "Value5" }).Result);

        Assert.Equal("value", assert.In(value, "value", new string[] { "Value3" }).ToResultReason().FirstOrDefault().Path);
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

        Assert.Equal("values", assert.NotIn(value, "values", new string[] { "Value2" }).ToResultReason().FirstOrDefault().Path);

        // Empty
        value = GetObject((name: "null", value: null), (name: "empty", value: Array.Empty<string>()));
        Assert.True(assert.NotIn(value, "null", new string[] { "Value1", "Value3" }).Result);
        Assert.True(assert.NotIn(value, "empty", new string[] { "Value1", "Value3" }).Result);
        Assert.True(assert.NotIn(value, "notValue", new string[] { "Value1", "Value3" }).Result);
    }

    [Fact]
    public void SetOf()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // Int
        var value = GetObject((name: "value", value: 3), (name: "values", value: new int[] { 3, 5 }));
        Assert.False(assert.SetOf(value, "value", new int[] { 3 }).Result);
        Assert.False(assert.SetOf(value, "values", new int[] { 3 }).Result);
        Assert.True(assert.SetOf(value, "values", new int[] { 5, 3 }).Result);
        Assert.False(assert.SetOf(value, "values", new int[] { 4 }).Result);
        Assert.False(assert.SetOf(value, "values", new int[] { 2, 3, 5 }).Result);

        // Float
        value = GetObject((name: "value", value: 3.0f), (name: "values", value: new float[] { 3f, 5f }));
        Assert.False(assert.SetOf(value, "value", new float[] { 3f }).Result);
        Assert.False(assert.SetOf(value, "values", new float[] { 3.0f }).Result);
        Assert.True(assert.SetOf(value, "values", new float[] { 5.0f, 3f }).Result);
        Assert.False(assert.SetOf(value, "values", new float[] { 4f }).Result);
        Assert.False(assert.SetOf(value, "values", new float[] { 2f, 3f, 5f }).Result);

        // String
        value = GetObject((name: "value", value: "value2"), (name: "values", value: new string[] { "value2", "value5" }));
        Assert.False(assert.SetOf(value, "value", new string[] { "value2" }).Result);
        Assert.False(assert.SetOf(value, "values", new string[] { "Value5" }).Result);
        Assert.True(assert.SetOf(value, "values", new string[] { "Value5", "Value2" }).Result);
        Assert.False(assert.SetOf(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
        Assert.False(assert.SetOf(value, "values", new string[] { "Value3", "Value5" }).Result);
        Assert.False(assert.SetOf(value, "values", new string[] { "Value2", "Value5" }, true).Result);
        Assert.True(assert.SetOf(value, "values", new string[] { "value2", "value5" }, true).Result);

        Assert.Equal("values", assert.SetOf(value, "values", new string[] { "Value2", "Value5" }, true).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void Subset()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // Int
        var value = GetObject((name: "value", value: 3), (name: "values", value: new int[] { 3, 5, 3 }));
        Assert.False(assert.Subset(value, "value", new int[] { 3 }).Result);

        Assert.True(assert.Subset(value, "values", new int[] { 3 }).Result);
        Assert.True(assert.Subset(value, "values", new int[] { 5, 3 }).Result);
        Assert.False(assert.Subset(value, "values", new int[] { 5, 3 }, false, true).Result);
        Assert.False(assert.Subset(value, "values", new int[] { 4 }).Result);
        Assert.False(assert.Subset(value, "values", new int[] { 2, 3, 5 }).Result);

        // Float
        value = GetObject((name: "value", value: 3.0f), (name: "values", value: new float[] { 3f, 5f, 5f }));
        Assert.False(assert.Subset(value, "value", new float[] { 3f }).Result);

        Assert.True(assert.Subset(value, "values", new float[] { 3.0f }).Result);
        Assert.True(assert.Subset(value, "values", new float[] { 5.0f, 3f }).Result);
        Assert.False(assert.Subset(value, "values", new float[] { 5.0f, 3f }, false, true).Result);
        Assert.False(assert.Subset(value, "values", new float[] { 4f }).Result);
        Assert.False(assert.Subset(value, "values", new float[] { 2f, 3f, 5f }).Result);

        // String
        value = GetObject((name: "value", value: "value2"), (name: "values", value: new string[] { "value2", "value5", "Value2" }));
        Assert.False(assert.Subset(value, "value", new string[] { "value2" }).Result);

        Assert.True(assert.Subset(value, "values", new string[] { "Value5" }).Result);
        Assert.True(assert.Subset(value, "values", new string[] { "Value5", "Value2" }).Result);
        Assert.False(assert.Subset(value, "values", new string[] { "Value5", "Value2" }, false, true).Result);
        Assert.False(assert.Subset(value, "values", new string[] { "VALUE1", "VALUE2", "VALUE3" }).Result);
        Assert.False(assert.Subset(value, "values", new string[] { "Value3", "Value5" }).Result);
        Assert.False(assert.Subset(value, "values", new string[] { "Value2", "Value5" }, true).Result);

        Assert.Equal("values", assert.Subset(value, "values", new string[] { "Value5", "Value2" }, false, true).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void Count()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // Array
        var value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.False(assert.Count(value, "value", 2).Result);
        Assert.True(assert.Count(value, "value", 3).Result);
        Assert.False(assert.Count(value, "value", 4).Result);
        Assert.False(assert.Count(value, "value", 0).Result);
        Assert.False(assert.Count(value, "value", -1).Result);

        Assert.Equal("value", assert.Count(value, "value", 2).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NotCount()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // Array
        var value = GetObject((name: "value", value: new string[] { "1", "2", "3" }));
        Assert.True(assert.NotCount(value, "value", 2).Result);
        Assert.False(assert.NotCount(value, "value", 3).Result);
        Assert.True(assert.NotCount(value, "value", 4).Result);
        Assert.True(assert.NotCount(value, "value", 0).Result);
        Assert.True(assert.NotCount(value, "value", -1).Result);

        Assert.Equal("value", assert.NotCount(value, "value", 3).ToResultReason().FirstOrDefault().Path);
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

        Assert.Equal("value", assert.Match(value, "value", "value2").ToResultReason().FirstOrDefault().Path);
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

        Assert.Equal("value", assert.NotMatch(value, "value", "Value[0-9]").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void Null()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "value4", value: 0)
        );
        Assert.False(assert.Null(value, "value").Result);
        Assert.True(assert.Null(value, "notValue").Result);
        Assert.True(assert.Null(value, "value2").Result);
        Assert.False(assert.Null(value, "value3").Result);
        Assert.False(assert.Null(value, "value4").Result);

        Assert.Equal("value", assert.Null(value, "value").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NotNull()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "value4", value: 0)
        );
        Assert.True(assert.NotNull(value, "value").Result);
        Assert.False(assert.NotNull(value, "value2").Result);
        Assert.True(assert.NotNull(value, "value3").Result);
        Assert.True(assert.NotNull(value, "value4").Result);
        Assert.False(assert.NotNull(value, "notValue").Result);

        Assert.Equal("value2", assert.NotNull(value, "value2").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NullOrEmpty()
    {
        SetContext();
        var assert = GetAssertionHelper();

        var value = GetObject(
            (name: "value", value: "Value1"),
            (name: "value2", value: null),
            (name: "value3", value: ""),
            (name: "value4", value: 0),
            (name: "value5", value: Array.Empty<string>())
        );
        Assert.False(assert.NullOrEmpty(value, "value").Result);
        Assert.True(assert.NullOrEmpty(value, "value2").Result);
        Assert.True(assert.NullOrEmpty(value, "value3").Result);
        Assert.False(assert.NullOrEmpty(value, "value4").Result);
        Assert.True(assert.NullOrEmpty(value, "value5").Result);
        Assert.True(assert.NullOrEmpty(value, "notValue").Result);

        Assert.Equal("value", assert.NullOrEmpty(value, "value").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void FileHeader()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var header = new string[] { "Copyright (c) Microsoft Corporation.", "Licensed under the MIT License." };

        // .ps1
        var value = GetObject((name: "FullName", value: GetSourcePath("FromFile.Rule.ps1")));
        Assert.True(assert.FileHeader(value, "FullName", header).Result);

        // .yaml
        value = GetObject((name: "FullName", value: GetSourcePath("Baseline.Rule.yaml")));
        Assert.False(assert.FileHeader(value, "FullName", header).Result);

        Assert.Equal("FullName", assert.FileHeader(value, "FullName", header).ToResultReason().FirstOrDefault().Path);

        // Dockerfile
        value = GetObject((name: "FullName", value: GetSourcePath("Dockerfile")));
        Assert.True(assert.FileHeader(value, "FullName", header).Result);

        // Jenkinsfile
        value = GetObject((name: "FullName", value: GetSourcePath("jenkinsfile")));
        Assert.True(assert.FileHeader(value, "FullName", header).Result);
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

        Assert.Equal("FullName", assert.FilePath(value, "FullName").ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void WithinPath()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // String
        var value = GetObject((name: "FullName", value: GetSourcePath("deployments/path/template.json")));
        Assert.True(AssertionResult(assert.WithinPath(value, "FullName", new string[] { "deployments/path/" }, caseSensitive: false)));
        Assert.True(AssertionResult(assert.WithinPath(value, "FullName", new string[] { "deployments\\path\\" }, caseSensitive: false)));
        Assert.False(AssertionResult(assert.WithinPath(value, "FullName", new string[] { "deployments/other/" }, caseSensitive: false)));
        Assert.False(AssertionResult(assert.WithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true)));

        // InputFileInfo
        value = GetObject((name: "FullName", value: new InputFileInfo(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployments/path/template.json"))));
        Assert.True(assert.WithinPath(value, "FullName", new string[] { "deployments/path/" }).Result);
        Assert.True(assert.WithinPath(value, "FullName", new string[] { "deployments\\path\\" }).Result);
        Assert.False(assert.WithinPath(value, "FullName", new string[] { "deployments/other/" }).Result);
        Assert.False(assert.WithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true).Result);

        // FileInfo
        value = GetObject((name: "FullName", value: new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployments/path/template.json"))));
        Assert.True(assert.WithinPath(value, "FullName", new string[] { "deployments/path/" }).Result);
        Assert.True(assert.WithinPath(value, "FullName", new string[] { "deployments\\path\\" }).Result);
        Assert.False(assert.WithinPath(value, "FullName", new string[] { "deployments/other/" }).Result);
        Assert.False(assert.WithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true).Result);

        Assert.Equal("FullName", assert.WithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NotWithinPath()
    {
        SetContext();
        var assert = GetAssertionHelper();

        // String
        var value = GetObject((name: "FullName", value: GetSourcePath("deployments/path/template.json")));
        Assert.False(AssertionResult(assert.NotWithinPath(value, "FullName", new string[] { "deployments/path/" }, caseSensitive: false)));
        Assert.False(AssertionResult(assert.NotWithinPath(value, "FullName", new string[] { "deployments\\path\\" }, caseSensitive: false)));
        Assert.True(AssertionResult(assert.NotWithinPath(value, "FullName", new string[] { "deployments/other/" }, caseSensitive: false)));
        Assert.True(AssertionResult(assert.NotWithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true)));

        // InputFileInfo
        value = GetObject((name: "FullName", value: new InputFileInfo(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployments/path/template.json"))));
        Assert.False(assert.NotWithinPath(value, "FullName", new string[] { "deployments/path/" }).Result);
        Assert.False(assert.NotWithinPath(value, "FullName", new string[] { "deployments\\path\\" }).Result);
        Assert.True(assert.NotWithinPath(value, "FullName", new string[] { "deployments/other/" }).Result);
        Assert.True(assert.NotWithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true).Result);

        // InputFileInfo
        value = GetObject((name: "FullName", value: new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployments/path/template.json"))));
        Assert.False(assert.NotWithinPath(value, "FullName", new string[] { "deployments/path/" }).Result);
        Assert.False(assert.NotWithinPath(value, "FullName", new string[] { "deployments\\path\\" }).Result);
        Assert.True(assert.NotWithinPath(value, "FullName", new string[] { "deployments/other/" }).Result);
        Assert.True(assert.NotWithinPath(value, "FullName", new string[] { "deployments/Path/" }, caseSensitive: true).Result);

        Assert.Equal("FullName", assert.NotWithinPath(value, "FullName", new string[] { "deployments/path/" }).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void Like()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.True(assert.Like(value, "name", ["123*", "ab*"]).Result);
        Assert.True(assert.Like(value, "name", ["123*", "ab*"], caseSensitive: true).Result);
        Assert.True(assert.Like(value, "name", ["AB*"]).Result);
        Assert.True(assert.Like(value, "name", ["*cd*"]).Result);
        Assert.True(assert.Like(value, "name", ["*fg*"]).Result);
        Assert.False(assert.Like(value, "name", ["abcdefgh"]).Result);
        Assert.False(assert.Like(value, "name", ["AB*"], caseSensitive: true).Result);
        Assert.False(assert.Like(value, "name", ["cd"]).Result);

        // Integer
        Assert.False(assert.Like(value, "value", ["12*"]).Result);

        Assert.Equal("value", assert.Like(value, "value", ["12*"]).ToResultReason().FirstOrDefault().Path);
    }

    [Fact]
    public void NotLike()
    {
        SetContext();
        var assert = GetAssertionHelper();
        var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

        // String
        Assert.False(assert.NotLike(value, "name", ["123*", "ab*"]).Result);
        Assert.False(assert.NotLike(value, "name", ["123*", "ab*"], caseSensitive: true).Result);
        Assert.False(assert.NotLike(value, "name", ["AB*"]).Result);
        Assert.False(assert.NotLike(value, "name", ["*cd*"]).Result);
        Assert.False(assert.NotLike(value, "name", ["*fg*"]).Result);
        Assert.True(assert.NotLike(value, "name", ["abcdefgh"]).Result);
        Assert.True(assert.NotLike(value, "name", ["AB*"], caseSensitive: true).Result);
        Assert.True(assert.NotLike(value, "name", ["cd"]).Result);

        // Integer
        Assert.True(assert.NotLike(value, "value", ["12*"]).Result);

        Assert.Equal("name", assert.NotLike(value, "name", ["123*", "ab*"]).ToResultReason().FirstOrDefault().Path);
    }

    #region Helper methods

    private void SetContext()
    {
        var context = GetPipelineContext();
        var runspace = new LegacyRunspaceContext(context);
        runspace.PushScope(RunspaceScope.Rule);
    }

    private static Runtime.Assert GetAssertionHelper()
    {
        return new Runtime.Assert();
    }

    private bool AssertionResult(AssertResult result)
    {
        if (!result.Result)
        {
            var reasons = result.GetReason();
            for (var i = 0; reasons != null && i < reasons.Length; i++)
                _Output.WriteLine(reasons[i]);
        }
        return result.Result;
    }

    #endregion Helper methods
}
