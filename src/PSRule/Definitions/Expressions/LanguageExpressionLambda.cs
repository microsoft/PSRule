// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal sealed class LanguageExpressionLambda(LanguageExpressionDescriptor descriptor) : LanguageExpression(descriptor)
{
    internal static readonly LanguageExpressionLambda True = new(new LanguageExpressionDescriptor("True", LanguageExpressionType.Condition, (context, info, args, o) => true));

    public LanguageExpressionFn Fn { get; } = descriptor.Fn;
}
