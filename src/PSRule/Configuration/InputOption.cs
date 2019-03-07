using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect how input types are processed.
    /// </summary>
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

        /// <summary>
        /// The input string format.
        /// </summary>
        [DefaultValue(null)]
        public InputFormat? Format { get; set; }

        /// <summary>
        /// The object path to a property to use instead of the pipeline object.
        /// </summary>
        [DefaultValue(null)]
        public string ObjectPath { get; set; }
    }
}
