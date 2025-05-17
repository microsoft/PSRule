// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Help;

[DebuggerDisplay("Type = {Type}, Text = {Text}")]
internal sealed class MarkdownToken
{
    public SourceExtent? Extent { get; set; }

    public MarkdownTokenType Type { get; set; }

    public string? Text { get; set; }

    public string? Meta { get; set; }

    public int Depth { get; set; }

    public MarkdownTokens Flag { get; set; }
}
