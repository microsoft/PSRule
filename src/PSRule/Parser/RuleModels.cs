// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System;

namespace PSRule.Parser
{
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

    internal sealed class RuleDocument
    {
        public RuleDocument(string name)
        {
            Name = name;
        }

        public readonly string Name;

        public TextBlock Synopsis;

        public TextBlock Description;

        public TextBlock Notes;

        public TextBlock Recommendation;

        public Link[] Links;

        public TagSet Annotations;
    }
}
