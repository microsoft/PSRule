using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for generating and formatting output.
    /// </summary>
    public sealed class OutputOption
    {
        private const ResultFormat DEFAULT_AS = ResultFormat.Detail;
        private const OutputEncoding DEFAULT_ENCODING = OutputEncoding.Default;
        private const OutputFormat DEFAULT_FORMAT = OutputFormat.None;
        private const OutputStyle DEFAULT_STYLE = OutputStyle.Client;

        public static readonly OutputOption Default = new OutputOption
        {
            As = DEFAULT_AS,
            Encoding = DEFAULT_ENCODING,
            Format = DEFAULT_FORMAT,
            Style = DEFAULT_STYLE
        };

        public OutputOption()
        {
            As = null;
            Culture = null;
            Encoding = null;
            Format = null;
            Path = null;
            Style = null;
        }

        public OutputOption(OutputOption option)
        {
            As = option.As;
            Culture = option.Culture;
            Encoding = option.Encoding;
            Format = option.Format;
            Path = option.Path;
            Style = option.Style;
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is OutputOption &&
                Equals(obj as OutputOption);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (As.HasValue ? As.Value.GetHashCode() : 0);
                hash = hash * 23 + (Culture != null ? Culture.GetHashCode() : 0);
                hash = hash * 23 + (Encoding.HasValue ? Encoding.Value.GetHashCode() : 0);
                hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
                hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
                hash = hash * 23 + (Style.HasValue ? Style.Value.GetHashCode() : 0);
                return hash;
            }
        }

        public bool Equals(OutputOption other)
        {
            return other != null &&
                As == other.As &&
                Culture == other.Culture &&
                Encoding == other.Encoding &&
                Format == other.Format &&
                Path == other.Path &&
                Style == other.Style;
        }

        /// <summary>
        /// The type of result to produce.
        /// </summary>
        [DefaultValue(null)]
        public ResultFormat? As { get; set; }

        [DefaultValue(null)]
        public string[] Culture { get; set; }

        /// <summary>
        /// The encoding to use when writing results to file.
        /// </summary>
        [DefaultValue(null)]
        public OutputEncoding? Encoding { get; set; }

        /// <summary>
        /// The output format.
        /// </summary>
        [DefaultValue(null)]
        public OutputFormat? Format { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }

        [DefaultValue(null)]
        public OutputStyle? Style { get; set; }
    }
}
