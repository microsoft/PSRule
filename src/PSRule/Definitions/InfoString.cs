// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions;

/// <summary>
/// A string formatted with plain text and/ or markdown.
/// </summary>
[DebuggerDisplay("{Text}")]
public sealed class InfoString
{
    private string _Text;
    private string _Markdown;

    internal InfoString() { }

    internal InfoString(string text, string markdown = null)
    {
        Text = text;
        Markdown = markdown ?? text;
    }

    /// <summary>
    /// Determine if the information string is empty.
    /// </summary>
    public bool HasValue
    {
        get { return Text != null || Markdown != null; }
    }

    /// <summary>
    /// A plain text representation.
    /// </summary>
    public string Text
    {
        get { return _Text; }
        set
        {
            if (!string.IsNullOrEmpty(value))
                _Text = value;
        }
    }

    /// <summary>
    /// A markdown formatted representation if set. Otherwise this is the same as <see cref="Text"/>.
    /// </summary>
    public string Markdown
    {
        get { return _Markdown; }
        set
        {
            if (!string.IsNullOrEmpty(value))
                _Markdown = value;
        }
    }

    /// <summary>
    /// Create an info string when not null or empty.
    /// </summary>
    internal static InfoString Create(string text, string markdown = null)
    {
        return string.IsNullOrEmpty(text) && string.IsNullOrEmpty(markdown) ? null : new InfoString(text, markdown);
    }
}
