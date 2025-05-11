// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

[DebuggerDisplay("Selector {Descriptor.Name}")]
internal sealed class LanguageOperator(LanguageExpressionDescriptor descriptor, LanguageExpression.PropertyBag properties) : LanguageExpression(descriptor)
{
    public LanguageExpression Subselector { get; set; }

    public PropertyBag Property { get; } = properties ?? [];

    public List<LanguageExpression> Children { get; } = [];

    public void Add(LanguageExpression item)
    {
        Children.Add(item);
    }
}
