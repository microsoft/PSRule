// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

public sealed class ExpressionContextTests : ContextBaseTests
{
    [Fact]
    public void Reason_WithText_ShouldAddReason()
    {
        var context = new ExpressionContext(new RunspaceContext(GetPipelineContext()), GetSourceFile("FromFile.Rule.yaml"), ResourceKind.Rule, new TestObject());
        context.Context.PushScope(RunspaceScope.Rule);
        var operand = Operand.FromPath("name");

        context.Reason(operand, "reason 1", null);
        context.Reason(operand, "reason 2", null);
        Assert.Equal(2, context.GetReasons().Length);
    }

    [Fact]
    public void Reason_WithDuplicate_ShouldNotAddReason()
    {
        var context = new ExpressionContext(new RunspaceContext(GetPipelineContext()), GetSourceFile("FromFile.Rule.yaml"), ResourceKind.Rule, new TestObject());
        context.Context.PushScope(RunspaceScope.Rule);
        var operand = Operand.FromPath("name");

        context.Reason(operand, "reason 1", null);
        context.Reason(operand, "reason 1", null);
        Assert.Single(context.GetReasons());
    }
}
