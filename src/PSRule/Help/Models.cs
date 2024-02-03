// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Help;

/// <summary>
/// Define options that determine how markdown will be rendered.
/// </summary>
[Flags()]
internal enum FormatOptions
{
    None = 0,

    /// <summary>
    /// Add a line break after headers.
    /// </summary>
    LineBreak = 1
}

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

/// <summary>
/// YAML link.
/// </summary>
internal sealed class Link
{
    public string Name;

    public string Uri;
}

internal interface IHelpDocument
{
    string Name { get; }

    InfoString Synopsis { get; set; }

    InfoString Description { get; set; }

    Link[] Links { get; set; }
}

internal sealed class RuleDocument : IHelpDocument
{
    public RuleDocument(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public InfoString Synopsis { get; set; }

    public InfoString Description { get; set; }

    public TextBlock Notes { get; set; }

    public InfoString Recommendation { get; set; }

    public Link[] Links { get; set; }

    public ResourceTags Annotations { get; set; }
}

internal sealed class ResourceHelpDocument : IHelpDocument
{
    public ResourceHelpDocument(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public InfoString Synopsis { get; set; }

    public InfoString Description { get; set; }

    public Link[] Links { get; set; }

    internal IResourceHelpInfo ToInfo()
    {
        return new ResourceHelpInfo(Name, Name, Synopsis, Description);
    }
}
