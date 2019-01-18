namespace PSRule.Configuration
{
    /// <summary>
    /// Options that specify the rules to evaluate or exclude and their configuration.
    /// </summary>
    public sealed class BaselineOption
    {
        public BaselineOption()
        {
            Configuration = new BaselineConfiguration();
        }

        public BaselineOption(BaselineOption option)
        {
            RuleName = option.RuleName;
            Exclude = option.Exclude;
            Configuration = new BaselineConfiguration(option.Configuration);
        }

        /// <summary>
        /// Rules to evaluate.
        /// </summary>
        /// <remarks>
        /// When not specified all rules will be evaluated.
        /// </remarks>
        public string[] RuleName { get; set; }

        /// <summary>
        /// Rules to exclude from being evaluate.
        /// </summary>
        /// <remarks>
        /// This option takes precedence over RuleName ig a rule is included in both list.
        /// </remarks>
        public string[] Exclude { get; set; }

        /// <summary>
        /// A set of configuration values that can be used within rule definitions.
        /// </summary>
        public BaselineConfiguration Configuration { get; set; }
    }
}
