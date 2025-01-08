// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

[DebuggerDisplay("Selector {Descriptor.Name}")]
internal sealed class LanguageCondition : LanguageExpression
{
    internal LanguageCondition(LanguageExpressionDescriptor descriptor, PropertyBag properties)
        : base(descriptor)
    {
        Property = properties ?? [];
    }

    public PropertyBag Property { get; }

    internal void Add(PropertyBag properties)
    {
        Property.AddUnique(properties);
    }
}
