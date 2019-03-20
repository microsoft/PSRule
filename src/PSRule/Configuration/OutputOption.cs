using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class OutputOption
    {
        private const OutputFormat DEFAULT_FORMAT = PSRule.Configuration.OutputFormat.None;

        public static readonly OutputOption Default = new OutputOption
        {
            Format = DEFAULT_FORMAT
        };

        public OutputOption()
        {
            Format = null;
        }

        public OutputOption(OutputOption option)
        {
            Format = option.Format;
        }

        /// <summary>
        /// The output format.
        /// </summary>
        [DefaultValue(null)]
        public OutputFormat? Format { get; set; }
    }
}
