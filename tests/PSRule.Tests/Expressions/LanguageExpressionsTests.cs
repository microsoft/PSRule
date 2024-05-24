// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

public sealed class LanguageExpressionsTests
{
    [Fact]
    public void StartsWith()
    {
        var context = GetContext();
        context.Setup(m => m.Reason(It.IsAny<IOperand>(), It.IsAny<string>(), It.Is<object[]>(
            p => p[0].ToString() == "'efg'" && p[1].ToString() == "'abc'"
        )));

        var info = GetInfo();
        var args = GetProperties(new LanguageExpression.PropertyBag {
            { "field", "." },
            { "startsWith", new string[] { "abc" } }
        });

        Assert.True(LanguageExpressions.StartsWith(context.Object, info, args, "abc"));
        Assert.False(LanguageExpressions.StartsWith(context.Object, info, args, "efg"));

        context.VerifyAll();
    }

    private static object[] GetProperties(LanguageExpression.PropertyBag properties)
    {
        return [
            properties
        ];
    }

    private static ExpressionInfo GetInfo()
    {
        return new ExpressionInfo("name");
    }

    private static Mock<IExpressionContext> GetContext()
    {
        return new Mock<IExpressionContext>(MockBehavior.Loose);
    }
}
