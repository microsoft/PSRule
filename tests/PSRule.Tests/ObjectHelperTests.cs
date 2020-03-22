// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Xunit;

namespace PSRule
{
    public sealed class ObjectHelperTests
    {
        [Fact]
        public void GetFieldTestPOCO()
        {
            var testObject = GetTestObject();

            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Name", caseSensitive: true, value: out object actual1);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Value.Value1", caseSensitive: false, value: out object actual2);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Metadata.'app.kubernetes.io/name'", caseSensitive: false, value: out object actual3);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Value2[1]", caseSensitive: false, value: out object actual4);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".", caseSensitive: true, value: out object actual5);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value2[1]", caseSensitive: false, value: out object actual6);

            Assert.Equal(expected: testObject.Name, actual: actual1);
            Assert.Equal(expected: testObject.Value.Value1, actual: actual2);
            Assert.Equal(expected: testObject.Metadata["app.kubernetes.io/name"], actual: actual3);
            Assert.Equal(expected: testObject.Value2[1], actual: actual4);
            Assert.Equal(expected: testObject, actual: actual5);
            Assert.Equal(expected: testObject.Value2[1], actual: actual6);
        }

        private static TestObject1 GetTestObject()
        {
            var result = new TestObject1 { Name = "TestObject1", Value = new TestObject2 { Value1 = "Value1" }, Value2 = new string[] { "1", "2" }, Metadata = new Hashtable() };
            result.Metadata.Add("app.kubernetes.io/name", "KubeName");
            return result;
        }

        public sealed class TestObject1
        {
            public string Name;

            public TestObject2 Value;

            public string[] Value2;

            public Hashtable Metadata;
        }

        public sealed class TestObject2
        {
            public string Value1;
        }
    }
}
