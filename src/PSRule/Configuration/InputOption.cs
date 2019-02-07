using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class InputOption
    {
        private const InputFormat DEFAULT_FORMAT = PSRule.Configuration.InputFormat.None;

        public static readonly InputOption Default = new InputOption
        {
            Format = DEFAULT_FORMAT
        };

        public InputOption()
        {
            Format = null;
        }

        public InputOption(InputOption option)
        {
            Format = option.Format;
        }

        [DefaultValue(null)]
        public InputFormat? Format { get; set; }
    }
}
