// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// Tests for <see cref="ExpressionContext"/>.
/// </summary>
public sealed class ExpressionContextTests : ContextBaseTests
{
    [Fact]
    public void Reason_WithText_ShouldAddReason()
    {
        var context = new ExpressionContext(new LegacyRunspaceContext(GetPipelineContext()), GetSourceFile("FromFile.Rule.yaml"), ResourceKind.Rule, new TargetObject(new System.Management.Automation.PSObject(new TestObject())));
        context.Context.PushScope(RunspaceScope.Rule);
        var operand = Operand.FromPath("name");

        context.Reason(operand, "reason 1", null);
        context.Reason(operand, "reason 2", null);
        Assert.Equal(2, context.GetReasons().Length);
    }

    [Fact]
    public void Reason_WithDuplicate_ShouldNotAddReason()
    {
        var context = new ExpressionContext(new LegacyRunspaceContext(GetPipelineContext()), GetSourceFile("FromFile.Rule.yaml"), ResourceKind.Rule, new TargetObject(new System.Management.Automation.PSObject(new TestObject())));
        context.Context.PushScope(RunspaceScope.Rule);
        var operand = Operand.FromPath("name");

        context.Reason(operand, "reason 1", null);
        context.Reason(operand, "reason 1", null);
        Assert.Single(context.GetReasons());
    }
}
