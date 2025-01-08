// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal abstract class LanguageExpression(LanguageExpressionDescriptor descriptor)
{
    internal sealed class PropertyBag : KeyMapDictionary<object>
    {
        public PropertyBag()
            : base() { }
    }

    public LanguageExpressionDescriptor Descriptor { get; } = descriptor;
}
