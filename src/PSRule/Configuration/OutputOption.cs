using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class OutputOption
    {
        private const ResultFormat DEFAULT_AS = PSRule.Configuration.ResultFormat.Detail;
        private const OutputFormat DEFAULT_FORMAT = PSRule.Configuration.OutputFormat.None;

        public static readonly OutputOption Default = new OutputOption
        {
            As = DEFAULT_AS,
            Format = DEFAULT_FORMAT
        };

        public OutputOption()
        {
            As = null;
            Format = null;
        }

        public OutputOption(OutputOption option)
        {
            As = option.As;
            Format = option.Format;
        }

        [DefaultValue(null)]
        public ResultFormat? As { get; set; }

        /// <summary>
        /// The output format.
        /// </summary>
        [DefaultValue(null)]
        public OutputFormat? Format { get; set; }
    }
}
