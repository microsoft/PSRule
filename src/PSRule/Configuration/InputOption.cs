using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class InputOption
    {
        private const InputFormat DEFAULT_FORMAT = PSRule.Configuration.InputFormat.None;
        private const string DEFAULT_OBJECTPATH = null;

        public static readonly InputOption Default = new InputOption
        {
            Format = DEFAULT_FORMAT,
            ObjectPath = DEFAULT_OBJECTPATH
        };

        public InputOption()
        {
            Format = null;
            ObjectPath = null;
        }

        public InputOption(InputOption option)
        {
            Format = option.Format;
            ObjectPath = option.ObjectPath;
        }

        [DefaultValue(null)]
        public InputFormat? Format { get; set; }

        [DefaultValue(null)]
        public string ObjectPath { get; set; }
    }
}
