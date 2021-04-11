﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace PSRule
{
    public sealed class ObjectHelperTests
    {
        [Fact]
        public void GetFieldPOCO()
        {
            var testObject = GetTestObject();

            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Name", caseSensitive: true, value: out object actual1);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Value.Value1", caseSensitive: false, value: out object actual2);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Metadata.'app.kubernetes.io/name'", caseSensitive: false, value: out object actual3);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Value2[1]", caseSensitive: false, value: out object actual4);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".", caseSensitive: true, value: out object actual5);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value2[1]", caseSensitive: false, value: out object actual6);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value3[1]", caseSensitive: false, value: out object actual7);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value4[0]", caseSensitive: false, value: out object actual8);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value5.name", caseSensitive: false, value: out object actual9);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: ".Value5[2]", caseSensitive: false, value: out object actual10);

            Assert.Equal(expected: testObject.Name, actual: actual1);
            Assert.Equal(expected: testObject.Value.Value1, actual: actual2);
            Assert.Equal(expected: testObject.Metadata["app.kubernetes.io/name"], actual: actual3);
            Assert.Equal(expected: testObject.Value2[1], actual: actual4);
            Assert.Equal(expected: testObject, actual: actual5);
            Assert.Equal(expected: testObject.Value2[1], actual: actual6);
            Assert.Equal(expected: testObject.Value3[1], actual: actual7);
            Assert.Equal(expected: "1", actual: actual8);
            Assert.Equal(expected: testObject.Value5["name"], actual: actual9);
            Assert.Equal(expected: testObject.Value5[2], actual: actual10);
        }

        [Fact]
        public void GetFieldDynamic()
        {
            var hashtable = new Hashtable
            {
                { "Name", "TestObject1" },
                { "Value", "Value1" }
            };
            var testObject = TagSet.FromHashtable(hashtable);

            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Name", caseSensitive: true, value: out object actual1);
            Runtime.ObjectHelper.GetField(bindingContext: null, targetObject: testObject, name: "Value", caseSensitive: true, value: out object actual2);

            Assert.Equal(expected: testObject["Name"], actual: actual1);
            Assert.Equal(expected: testObject["Value"], actual: actual2);
        }

        private static TestObject1 GetTestObject()
        {
            var value5 = new TestObject3();
            value5["name"] = "1";
            value5[2] = "2";

            var result = new TestObject1
            {
                Name = "TestObject1",
                Value = new TestObject2 { Value1 = "Value1" },
                Value2 = new string[] { "1", "2" },
                Metadata = new Hashtable(),
                Value3 = new List<string>(new string[] { "1", "2" }),
                Value4 = new List<string>(new string[] { "1" }).AsReadOnly(),
                Value5 = value5
            };
            result.Metadata.Add("app.kubernetes.io/name", "KubeName");
            return result;
        }

        public sealed class TestObject1
        {
            public string Name;

            public TestObject2 Value;

            public string[] Value2;

            public Hashtable Metadata;

            public IList<string> Value3;

            public ICollection<string> Value4;

            public TestObject3 Value5;
        }

        public sealed class TestObject2
        {
            public string Value1;
        }

        public sealed class TestObject3
        {
            private readonly Dictionary<object, object> _Internal;

            public TestObject3()
            {
                _Internal = new Dictionary<object, object>();
            }

            public object this[object key]
            {
                get => _Internal[key];
                set => _Internal[key] = value;
            }
        }
    }
}
