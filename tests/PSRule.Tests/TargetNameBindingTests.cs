﻿using PSRule.Configuration;
using PSRule.Pipeline;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    public sealed class TargetNameBindingTests
    {
        internal class TestModel1
        {
            public string NotName { get; set; }
        }

        internal class TestModel2
        {
            public string NotName { get; set; }
        }

        [Fact]
        public void UnboundObjectTargetName()
        {
            var testObject1 = new TestModel1 { NotName = "TestObject1" };
            var testObject2 = new TestModel2 { NotName = "TestObject1" };
            var pso1 = PSObject.AsPSObject(testObject1);
            var pso2 = PSObject.AsPSObject(testObject2);

            PipelineContext.CurrentThread = PipelineContext.New(logger: null, option: new PSRuleOption(), bindTargetName: null, bindTargetType: null);
            var actual1 = PipelineHookActions.DefaultTargetNameBinding(pso1);
            var actual2 = PipelineHookActions.DefaultTargetNameBinding(pso2);

            Assert.Equal(expected: "f209c623345144be61087d91f30c17b01c6e86d2", actual: actual1);
            Assert.Equal(expected: "f209c623345144be61087d91f30c17b01c6e86d2", actual: actual2);
        }
    }
}
