using PSRule.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PSRule
{
    public sealed class ObjectHelperTests
    {
        public sealed class TestObject1
        {
            public string Name;

            public TestObject2 Value;
        }

        public sealed class TestObject2
        {
            public string Value1;
        }

        [Fact]
        public void GetFieldTestPOCO()
        {
            var testObject = new TestObject1 { Name = "TestObject1", Value = new TestObject2 { Value1 = "Value1" } };

            ObjectHelper.GetField(targetObject: testObject, name: "Name", caseSensitive: false, value: out object actual1);
            ObjectHelper.GetField(targetObject: testObject, name: "Value.Value1", caseSensitive: false, value: out object actual2);

            Assert.Equal(expected: testObject.Name, actual: actual1);
            Assert.Equal(expected: testObject.Value.Value1, actual: actual2);
        }
    }
}
