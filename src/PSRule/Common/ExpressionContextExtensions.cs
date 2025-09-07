// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

internal static class ExpressionContextExtensions
{
    public static bool ExpressionTrace(this IExpressionContext context, string name, object operand, object value)
    {
        var type = context.Kind == Definitions.ResourceKind.Rule ? 'R' : 'S';
        context.Logger.LogDebug(EventId.None, PSRuleResources.LanguageExpressionTraceP3, type, name, operand, value);
        return true;
    }

    public static bool ExpressionTrace(this IExpressionContext context, string name, object value)
    {
        var type = context.Kind == Definitions.ResourceKind.Rule ? 'R' : 'S';
        context.Logger.LogDebug(EventId.None, PSRuleResources.LanguageExpressionTraceP2, type, name, value);
        return true;
    }
}
