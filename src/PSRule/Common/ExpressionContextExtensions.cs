// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Resources;

namespace PSRule;

internal static class ExpressionContextExtensions
{
    public static bool ExpressionTrace(this ExpressionContext context, string name, object operand, object value)
    {
        var type = context.Kind == Definitions.ResourceKind.Rule ? 'R' : 'S';
        context.Debug(PSRuleResources.LanguageExpressionTraceP3, type, name, operand, value);
        return true;
    }

    public static bool ExpressionTrace(this ExpressionContext context, string name, object value)
    {
        var type = context.Kind == Definitions.ResourceKind.Rule ? 'R' : 'S';
        context.Debug(PSRuleResources.LanguageExpressionTraceP2, type, name, value);
        return true;
    }
}
