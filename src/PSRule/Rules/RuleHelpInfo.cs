namespace PSRule.Rules
{
    /// <summary>
    /// Output view helper class for rule help.
    /// </summary>
    public sealed class RuleHelpInfo
    {
        /// <summary>
        /// The name of the rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The synopsis of the rule.
        /// </summary>
        public string Synopsis { get; set; }

        /// <summary>
        /// The recommendation for the rule.
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// Additional notes for the rule.
        /// </summary>
        public string Notes { get; set; }
    }
}
