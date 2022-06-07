// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Runtime.ObjectPath;
using Xunit;

namespace PSRule
{
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
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out object actual1));
            var actualArray = actual1 as object[];
            Assert.NotNull(actualArray);
            Assert.Equal(2, actualArray.Length);

            expression = PathExpression.Create("$[-1].TargetName");
            Assert.False(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out object actual2));
            Assert.False(actual2 is object[]);
            Assert.NotNull(actual2);
            Assert.Equal("TestObject2", actual2);

            expression = PathExpression.Create("$[-3].TargetName");
            Assert.False(expression.IsArray);
            Assert.False(expression.TryGet(testObject, false, out object actual3));
            Assert.Null(actual3);

            expression = PathExpression.Create("$[*].Spec.Properties.array[*].id");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out object[] actual4));
            Assert.NotNull(actual4);
            Assert.Equal(4, actual4.Length);
            Assert.Equal("1", actual4[0]);
            Assert.Equal("2", actual4[1]);
            Assert.Equal("1", actual4[2]);
            Assert.Equal("2", actual4[3]);
        }

        [Fact]
        public void WithMemberCase()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[*].spec.Properties.array[*].id");
            Assert.True(expression.TryGet(testObject, false, out object[] actual));
            Assert.NotNull(actual);
            Assert.Equal(4, actual.Length);

            expression = PathExpression.Create("$[*].spec.Properties.array[*].id");
            Assert.False(expression.TryGet(testObject, true, out actual));
            Assert.Null(actual);

            expression = PathExpression.Create("$[0].targetName");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);

            expression = PathExpression.Create("$[0]+targetName");
            Assert.False(expression.TryGet(testObject, false, out actual));
            Assert.Null(actual);

            expression = PathExpression.Create("$[0].targetName");
            Assert.False(expression.TryGet(testObject, true, out actual));
            Assert.Null(actual);

            expression = PathExpression.Create("$[0]+targetName");
            Assert.True(expression.TryGet(testObject, true, out actual));
            Assert.NotNull(actual);
        }

        [Fact]
        public void WithFilter()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1')].id");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out object[] actual));
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.Equal("1", actual[0]);
            Assert.Equal("1", actual[1]);

            expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id==1)].id");
            Assert.True(expression.IsArray);
            Assert.False(expression.TryGet(testObject, false, out object[] _));

            expression = PathExpression.Create("$[?@.TargetName == 'TestObject1'].TargetName");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0]);
        }

        [Fact]
        public void WithOrFilter()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' || @.id=='2')].id");
            Assert.True(expression.TryGet(testObject, false, out object[] actual));
            Assert.NotNull(actual);
            Assert.Equal(4, actual.Length);
            Assert.Equal("1", actual[0]);
            Assert.Equal("2", actual[1]);
            Assert.Equal("1", actual[2]);
            Assert.Equal("2", actual[3]);
        }

        [Fact]
        public void WithAndFilter()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' && @.id=='2')].id");
            Assert.False(expression.TryGet(testObject, false, out object[] actual));
            Assert.Null(actual);

            expression = PathExpression.Create("$[*].Spec.Properties.array[?(@.id=='1' && @.id=='1')].id");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.Equal("1", actual[0]);
            Assert.Equal("1", actual[1]);
        }

        [Fact]
        public void WithCombinedFilter()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[?(@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && @Spec.Properties.Value1].TargetName");
            Assert.True(expression.TryGet(testObject, false, out object[] actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0]);

            expression = PathExpression.Create("$[?((@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && @Spec.Properties.Value2)].TargetName");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("TestObject2", actual[0]);

            expression = PathExpression.Create("$[?(@Spec.Properties.Kind == 'Test' || @Spec.Properties.Kind == 'Test2') && (@Spec.Properties.Value1 || @Spec.Properties.Value2)].TargetName");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0]);
            Assert.Equal("TestObject2", actual[1]);
        }

        [Fact]
        public void WithExistsFilter()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[?@.Spec.Properties.Kind].TargetName");
            Assert.True(expression.TryGet(testObject, false, out object[] actual));
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0]);
            Assert.Equal("TestObject2", actual[1]);

            expression = PathExpression.Create("$[?@.Spec.Properties.Value1].TargetName");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("TestObject1", actual[0]);

            expression = PathExpression.Create("$[?@.Spec.Properties.Value2].TargetName");
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("TestObject2", actual[0]);
        }

        [Fact]
        public void WithSlice()
        {
            var testObject = GetJsonContent();

            var expression = PathExpression.Create("$[0].Spec.Properties.array[:1].id");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, true, out object[] actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("1", actual[0]);

            expression = PathExpression.Create("$[0].spec.properties.array[:1].id");
            Assert.True(expression.IsArray);
            Assert.False(expression.TryGet(testObject, true, out actual));
            Assert.Null(actual);

            expression = PathExpression.Create("$[0].spec.properties.array2[::]");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Equal("1", actual[0]);
            Assert.Equal("2", actual[1]);
            Assert.Equal("3", actual[2]);

            expression = PathExpression.Create("$[0].spec.properties.array2[::-1]");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Equal("3", actual[0]);
            Assert.Equal("2", actual[1]);
            Assert.Equal("1", actual[2]);

            expression = PathExpression.Create("$[0].spec.properties.array2[:1:-1]");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Empty(actual);

            expression = PathExpression.Create("$[0].spec.properties.array2[2:1:-1]");
            Assert.True(expression.IsArray);
            Assert.True(expression.TryGet(testObject, false, out actual));
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal("3", actual[0]);
        }

        #region Helper methods

        private object GetJsonContent()
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

        #endregion Helper methods
    }
}
