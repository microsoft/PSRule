// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

/// <summary>
/// Markdown text content.
/// </summary>
internal sealed class TextBlock
{
    /// <summary>
    /// The text of the section body.
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// Additional options that determine how the section will be formated when rendering markdown.
    /// </summary>
    public readonly FormatOptions FormatOption;

    public TextBlock(string text, FormatOptions formatOption = FormatOptions.None)
    {
        Text = text;
        FormatOption = formatOption;
    }

    public override string ToString()
    {
        return Text;
    }
}
