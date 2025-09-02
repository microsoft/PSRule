// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

[DebuggerDisplay("Selector If")]
internal sealed class LanguageIf(LanguageExpression expression) : LanguageExpression(null)
{
    public LanguageExpression Expression { get; set; } = expression;
}
