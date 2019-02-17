using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options tht affect property binding of TargetName.
    /// </summary>
    public sealed class BindingOption
    {
        private const bool DEFAULT_IGNORECASE = false;

        public static readonly BindingOption Default = new BindingOption
        {
            IgnoreCase = DEFAULT_IGNORECASE
        };

        public BindingOption()
        {
            IgnoreCase = null;
            TargetName = null;
        }

        public BindingOption(BindingOption option)
        {
            IgnoreCase = option.IgnoreCase;
            TargetName = option.TargetName;
        }

        /// <summary>
        /// Determines if custom binding uses ignores case when matching properties.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreCase { get; set; }

        /// <summary>
        /// One or more property names to use to bind TargetName.
        /// </summary>
        [DefaultValue(null)]
        public string[] TargetName { get; set; }
    }
}
