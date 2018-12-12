namespace PSRule.Rules
{
    public interface IRuleRecord
    {
        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// The outcome after processing the rule.
        /// </summary>
        RuleOutcome Outcome { get; }

        bool IsSuccess();
    }
}