using PSRule.Rules;

namespace PSRule.Parser
{
    /// <summary>
    /// YAML text content.
    /// </summary>
    internal sealed class Body
    {
        public Body(string text, SectionFormatOption formatOption = SectionFormatOption.None)
        {
            Text = text;
            FormatOption = formatOption;
        }

        /// <summary>
        /// The text of the section body.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Additional options that determine how the section will be formated when rendering markdown.
        /// </summary>
        public SectionFormatOption FormatOption { get; set; }

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

    /// <summary>
    /// YAML code block.
    /// </summary>
    internal sealed class CodeBlock
    {
        public CodeBlock(string text, string meta)
        {

        }
    }

    internal sealed class RuleRecommendation
    {
        public string Title { get; set; }

        public object FormatOption { get; set; }

        public string Introduction { get; set; }

        public CodeBlock[] Code { get; set; }

        public string Remarks { get; set; }
    }

    internal sealed class RuleDocument
    {
        public string Name;

        public Body Synopsis;
        
        public Body Notes;

        public RuleRecommendation[] Recommendation;

        public Link[] Links;

        public TagSet Annotations;
    }
}
