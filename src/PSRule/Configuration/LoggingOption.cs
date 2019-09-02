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
            RulePass = DEFAULT_RULEPASS,
            LimitVerbose = null,
            LimitDebug = null
        };

        public LoggingOption()
        {
            LimitDebug = null;
            LimitVerbose = null;
            RuleFail = null;
            RulePass = null;
        }

        public LoggingOption(LoggingOption option)
        {
            LimitDebug = option.LimitDebug;
            LimitVerbose = option.LimitVerbose;
            RuleFail = option.RuleFail;
            RulePass = option.RulePass;
        }

        /// <summary>
        /// Limits debug messages to a list of named debug scopes.
        /// </summary>
        [DefaultValue(null)]
        public string[] LimitDebug { get; set; }

        /// <summary>
        /// Limits verbose messages to a list of named verbose scopes.
        /// </summary>
        [DefaultValue(null)]
        public string[] LimitVerbose { get; set; }

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
