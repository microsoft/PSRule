using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for logging outcomes to a informational streams.
    /// </summary>
    public sealed class LoggingOption
    {
        private const OutcomeLogStream DEFAULT_RULEFAIL = OutcomeLogStream.None;
        private const OutcomeLogStream DEFAULT_RULEPASS = OutcomeLogStream.None;

        public static readonly LoggingOption Default = new LoggingOption
        {
            RuleFail = DEFAULT_RULEFAIL,
            RulePass = DEFAULT_RULEPASS
        };

        public LoggingOption()
        {
            RuleFail = null;
            RulePass = null;
        }

        public LoggingOption(LoggingOption option)
        {
            RuleFail = option.RuleFail;
            RulePass = option.RulePass;
        }

        /// <summary>
        /// Log fail outcomes for each rule to a specific informational stream.
        /// </summary>
        [DefaultValue(null)]
        public OutcomeLogStream? RuleFail { get; set; }

        /// <summary>
        /// Log pass outcomes for each rule to a specific informational stream.
        /// </summary>
        [DefaultValue(null)]
        public OutcomeLogStream? RulePass { get; set; }
    }
}
