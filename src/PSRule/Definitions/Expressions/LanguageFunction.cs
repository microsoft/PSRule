// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

[DebuggerDisplay("Selector {Descriptor.Name}")]
internal sealed class LanguageFunction : LanguageExpression
{
    internal LanguageFunction(LanguageExpressionDescriptor descriptor)
        : base(descriptor) { }
}
