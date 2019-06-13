using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class OutputOption
    {
        private const ResultFormat DEFAULT_AS = ResultFormat.Detail;
        private const OutputEncoding DEFAULT_ENCODING = OutputEncoding.Default;
        private const OutputFormat DEFAULT_FORMAT = OutputFormat.None;

        public static readonly OutputOption Default = new OutputOption
        {
            As = DEFAULT_AS,
            Encoding = DEFAULT_ENCODING,
            Format = DEFAULT_FORMAT
        };

        public OutputOption()
        {
            As = null;
            Encoding = null;
            Format = null;
            Path = null;
        }

        public OutputOption(OutputOption option)
        {
            As = option.As;
            Encoding = option.Encoding;
            Format = option.Format;
            Path = option.Path;
        }

        /// <summary>
        /// The type of result to produce.
        /// </summary>
        [DefaultValue(null)]
        public ResultFormat? As { get; set; }

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
    }
}
