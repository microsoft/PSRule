// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule;

public sealed class TargetNameBindingTests : ContextBaseTests
{
    internal class TestModel1
    {
        public string? NotName { get; set; }
    }

    internal class TestModel2
    {
        public string? NotName { get; set; }
    }

    internal class TestModel3 : ITargetInfo
    {
        public string? Name { get; set; }

        string ITargetInfo.Name => "TestModel3";

        string ITargetInfo.Type => "TestModel3";

        TargetSourceInfo? ITargetInfo.Source => null;
    }

    [Fact]
    public void UnboundObjectTargetName()
    {
        var testObject1 = new TestModel1 { NotName = "TestObject1" };
        var testObject2 = new TestModel2 { NotName = "TestObject1" };
        var pso1 = PSObject.AsPSObject(testObject1);
        var pso2 = PSObject.AsPSObject(testObject2);

        // SHA512
        PipelineContext.CurrentThread = GetPipelineContext(option: GetOption());

        Assert.Equal("f3d2f8ce966af96a8d320e8f5c088604324885a0d02f44b174", PipelineHookActions.BindTargetName(null, false, pso1, out _));
        Assert.Equal("f3d2f8ce966af96a8d320e8f5c088604324885a0d02f44b174", PipelineHookActions.BindTargetName(null, false, pso2, out _));

        // SHA256
        PipelineContext.CurrentThread = GetPipelineContext(option: GetOption(HashAlgorithm.SHA256));

        Assert.Equal("67327c8cd8622d17cf1702a76cbbb685e9ef260ce39c9f6779", PipelineHookActions.BindTargetName(null, false, pso1, out _));
        Assert.Equal("67327c8cd8622d17cf1702a76cbbb685e9ef260ce39c9f6779", PipelineHookActions.BindTargetName(null, false, pso2, out _));
    }

    #region Helper methods

    private static PSRuleOption GetOption(HashAlgorithm? hashAlgorithm = null)
    {
        var option = new PSRuleOption();
        if (hashAlgorithm != null)
            option.Execution.HashAlgorithm = hashAlgorithm;

        return option;
    }

    #endregion Helper methods
}
