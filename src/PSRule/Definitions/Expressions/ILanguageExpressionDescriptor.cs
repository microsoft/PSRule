// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal interface ILanguageExpressionDescriptor
{
    string Name { get; }

    LanguageExpressionType Type { get; }

    LanguageExpression? CreateInstance(ISourceFile source, LanguageExpression.PropertyBag properties);
}
