// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal sealed class LanguageExpressionDescriptor(string name, LanguageExpressionType type, LanguageExpressionFn fn)
    : ILanguageExpressionDescriptor
{
    public string Name { get; } = name;

    public LanguageExpressionType Type { get; } = type;

    public LanguageExpressionFn Fn { get; } = fn;

    public LanguageExpression? CreateInstance(ISourceFile source, LanguageExpression.PropertyBag properties)
    {
        if (Type == LanguageExpressionType.Operator)
            return new LanguageOperator(this, properties);

        if (Type == LanguageExpressionType.Condition)
            return new LanguageCondition(this, properties);

        return Type == LanguageExpressionType.Function ? new LanguageFunction(this) : null;
    }
}
