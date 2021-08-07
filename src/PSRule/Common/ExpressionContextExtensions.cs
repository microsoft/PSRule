// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Resources;

namespace PSRule
{
    internal static class ExpressionContextExtensions
    {
        public static void ExpressionTrace(this ExpressionContext context, string name, object operand, object value)
        {
            context.Debug(PSRuleResources.LanguageExpressionTrace, name, operand, value);
        }
    }
}
