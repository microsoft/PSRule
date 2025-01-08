// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

[DebuggerDisplay("Selector {Descriptor.Name}")]
internal sealed class LanguageOperator : LanguageExpression
{
    internal LanguageOperator(LanguageExpressionDescriptor descriptor, PropertyBag properties)
        : base(descriptor)
    {
        Property = properties ?? [];
        Children = [];
    }

    public LanguageExpression Subselector { get; set; }

    public PropertyBag Property { get; }

    public List<LanguageExpression> Children { get; }

    public void Add(LanguageExpression item)
    {
        Children.Add(item);
    }
}
