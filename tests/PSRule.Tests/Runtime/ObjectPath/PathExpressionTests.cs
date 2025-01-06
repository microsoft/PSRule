// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// Tests for a JSONPath expression.
/// </summary>
public sealed class PathExpressionTests
{
    [Fact]
    public void Basic()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out object actual1));
        var actualArray = actual1 as object[];
        Xunit.Assert.NotNull(actualArray);
        Xunit.Assert.Equal(2, actualArray.Length);

        expression = PathExpression.Create("$[-1].TargetName");
        Xunit.Assert.False(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out object actual2));
        Xunit.Assert.False(actual2 is object[]);
        Xunit.Assert.NotNull(actual2);
        Xunit.Assert.Equal("TestObject2", actual2);

        expression = PathExpression.Create("$[-3].TargetName");
        Xunit.Assert.False(expression.IsArray);
        Xunit.Assert.False(expression.TryGet(testObject, false, out object actual3));
        Xunit.Assert.Null(actual3);

        expression = PathExpression.Create("$[*].Spec.Properties.array[*].id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual4));
        Xunit.Assert.NotNull(actual4);
        Xunit.Assert.Equal(4, actual4.Length);
        Xunit.Assert.Equal("1", actual4[0]);
        Xunit.Assert.Equal("2", actual4[1]);
        Xunit.Assert.Equal("1", actual4[2]);
        Xunit.Assert.Equal("2", actual4[3]);
    }

    [Fact]
    public void WithMemberCase()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*].spec.Properties.array[*].id");
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(4, actual.Length);

        expression = PathExpression.Create("$[*].spec.Properties.array[*].id");
        Xunit.Assert.False(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[0].targetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);

        expression = PathExpression.Create("$[0]+targetName");
        Xunit.Assert.False(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[0].targetName");
        Xunit.Assert.False(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[0]+targetName");
        Xunit.Assert.True(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.NotNull(actual);
    }

    [Fact]
    public void WithFilter()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1')].id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(2, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("1", actual[1]);

        expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id==1)].id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.False(expression.TryGet(testObject, false, out object[] _));

        expression = PathExpression.Create("$[?@.TargetName == 'TestObject1'].TargetName");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject1", actual[0]);
    }

    [Fact]
    public void WithOrFilter()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' || @.id=='2')].id");
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(4, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("2", actual[1]);
        Xunit.Assert.Equal("1", actual[2]);
        Xunit.Assert.Equal("2", actual[3]);

        expression = PathExpression.Create("$[*].Spec.Properties.array2[?(@ == '1' || @ == '2' || @ == '3')]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(6, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("2", actual[1]);
        Xunit.Assert.Equal("3", actual[2]);
        Xunit.Assert.Equal("1", actual[3]);
        Xunit.Assert.Equal("2", actual[4]);
        Xunit.Assert.Equal("3", actual[5]);
    }

    [Fact]
    public void WithAndFilter()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' && @.id=='2')].id");
        Xunit.Assert.False(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' && @.id=='1')].id");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(2, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("1", actual[1]);

        expression = PathExpression.Create("$[*].Spec.Properties.array2[?(@ == '1' && @ == '2')]");
        Xunit.Assert.False(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[*].Spec.Properties.array2[?(@ == '1' && @ == '1')]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(2, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("1", actual[1]);
    }

    [Fact]
    public void WithCombinedFilter()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[?(@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && @Spec.Properties.Value1].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject1", actual[0]);

        expression = PathExpression.Create("$[?((@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && @Spec.Properties.Value2)].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject2", actual[0]);

        expression = PathExpression.Create("$[?(@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && (@Spec.Properties.Value1 || @Spec.Properties.Value2)].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(2, actual.Length);
        Xunit.Assert.Equal("TestObject1", actual[0]);
        Xunit.Assert.Equal("TestObject2", actual[1]);
    }

    [Fact]
    public void WithNullValue()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[?@.spec.properties.from == 'abc'].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject2", actual[0]);
    }

    [Fact]
    public void WithExistsFilter()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[?@.Spec.Properties.Kind].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(2, actual.Length);
        Xunit.Assert.Equal("TestObject1", actual[0]);
        Xunit.Assert.Equal("TestObject2", actual[1]);

        expression = PathExpression.Create("$[?@.Spec.Properties.Value1].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject1", actual[0]);

        expression = PathExpression.Create("$[?@.Spec.Properties.Value2].TargetName");
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject2", actual[0]);
    }

    [Fact]
    public void WithSlice()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[0].Spec.Properties.array[:1].id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, true, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("1", actual[0]);

        expression = PathExpression.Create("$[0].spec.properties.array[:1].id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.False(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.Null(actual);

        expression = PathExpression.Create("$[0].spec.properties.array2[::]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("2", actual[1]);
        Xunit.Assert.Equal("3", actual[2]);

        expression = PathExpression.Create("$[0].spec.properties.array2[::-1]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal("3", actual[0]);
        Xunit.Assert.Equal("2", actual[1]);
        Xunit.Assert.Equal("1", actual[2]);

        expression = PathExpression.Create("$[0].spec.properties.array2[:1:-1]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Empty(actual);

        expression = PathExpression.Create("$[0].spec.properties.array2[2:1:-1]");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, false, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("3", actual[0]);
    }

    [Fact]
    public void WithDescendant()
    {
        var testObject = GetJsonContent();

        var expression = PathExpression.Create("$[*]..Value2");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, true, out object[] actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal(2L, actual[0]);

        expression = PathExpression.Create("$[*]..id");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Equal(4, actual.Length);
        Xunit.Assert.Equal("1", actual[0]);
        Xunit.Assert.Equal("2", actual[1]);
        Xunit.Assert.Equal("1", actual[2]);
        Xunit.Assert.Equal("2", actual[3]);

        expression = PathExpression.Create("$[?@..Value2].TargetName");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject, true, out actual));
        Xunit.Assert.NotNull(actual);
        Xunit.Assert.Single(actual);
        Xunit.Assert.Equal("TestObject2", actual[0]);

        // Handle exception cases
        var pso = GetPSObjectContent();
        expression = PathExpression.Create("$..value");
        Xunit.Assert.False(expression.TryGet(pso, true, out object[] _));
    }

    [Fact]
    public void WithArrayExpand()
    {
        var testObject = GetJsonContent() as JArray;
        var expression = PathExpression.Create("Spec.config.[*][*].prop[*].public");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject[0], true, out object[] actual));
        Xunit.Assert.Equal(true, actual[0]);
        Xunit.Assert.True(expression.TryGet(testObject[1], true, out actual));
        Xunit.Assert.Equal(false, actual[0]);

        expression = PathExpression.Create("Spec.config[*][*].prop[*].public");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject[0], true, out actual));
        Xunit.Assert.Equal(true, actual[0]);
        Xunit.Assert.True(expression.TryGet(testObject[1], true, out actual));
        Xunit.Assert.Equal(false, actual[0]);

        expression = PathExpression.Create("Spec.config.[*].[*].prop[*].public");
        Xunit.Assert.True(expression.IsArray);
        Xunit.Assert.True(expression.TryGet(testObject[0], true, out actual));
        Xunit.Assert.Equal(true, actual[0]);
        Xunit.Assert.True(expression.TryGet(testObject[1], true, out actual));
        Xunit.Assert.Equal(false, actual[0]);
    }

    [Theory]
    [InlineData("policies")]
    [InlineData("policies.inbound")]
    [InlineData("policies.inbound.ip-filter")]
    [InlineData("policies.inbound.ip-filter.address-range")]
    [InlineData("policies.inbound.set-header[0]")]
    [InlineData("policies.inbound.set-header[2]")]
    public void TryGet_WithXmlContent_ShouldReturnExpectedItem(string path)
    {
        var testObject = GetXmlContent();
        var expression = PathExpression.Create(path);

        Xunit.Assert.True(expression.TryGet(testObject, false, out object actual));
        Xunit.Assert.True(actual is XmlNode);
    }

    [Theory]
    [InlineData("policies.inbound.ip-filter.action", "allow")]
    [InlineData("policies.inbound.ip-filter.address-range.from", "0.0.0.0")]
    [InlineData("policies.inbound.set-header[0].exists-action", "override")]
    [InlineData("policies.inbound.set-header[0].value.InnerText", "20")]
    [InlineData("policies.inbound.set-header[2].value[1].InnerText", "value2")]
    [InlineData("policies.inbound.set-header[-1].value[-2].InnerText", "value1")]
    public void TryGet_WithXmlContent_ShouldReturnReadProperty(string path, object value)
    {
        var testObject = GetXmlContent();
        var expression = PathExpression.Create(path);

        Xunit.Assert.True(expression.TryGet(testObject, false, out object actual));
        var text = actual as string;

        Xunit.Assert.Equal(value, text);
    }

    [Theory]
    [InlineData("policies.inbound.base")]
    [InlineData("policies.base")]
    public void TryGet_WithXmlWithoutContentPath_ShouldNotReturnAnyItem(string path)
    {
        var testObject = GetXmlContent();
        var expression = PathExpression.Create(path);

        expression.TryGet(testObject, false, out object actual);
        Xunit.Assert.Null(actual);
    }

    #region Helper methods

    private static JToken GetJsonContent()
    {
        var settings = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore
        };
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.json");
        using var stream = new StreamReader(path);
        using var reader = new JsonTextReader(stream);
        return JToken.Load(reader, settings);
    }

    private static PSObject GetPSObjectContent()
    {
        var result = new PSObject();
        result.Properties.Add(new PSNoteProperty("string", ""));
        result.Properties.Add(new PSNoteProperty("date", DateTime.Now));
        result.Properties.Add(new PSNoteProperty("int", 0));
        return result;
    }

    private static XmlDocument GetXmlContent()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.xml");
        var doc = new XmlDocument();
        doc.Load(path);
        return doc;
    }

    #endregion Helper methods
}
