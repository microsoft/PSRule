using PSRule.Host;
using System.Diagnostics;

namespace PSRule.Rules
{
    public delegate bool RulePrecondition();

    public delegate RuleConditionResult RuleCondition();

    /// <summary>
    /// Define an instance of a rule block. Each rule block has a unique id.
    /// </summary>
    [DebuggerDisplay("{Id} @{SourcePath}")]
    public sealed class RuleBlock : ILanguageBlock, IDependencyTarget
    {
        public RuleBlock(string ruleId)
        {
            RuleId = ruleId;
        }

        public string SourcePath { get; set; }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        public string RuleId { get; private set; }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A precondition that if set, must evaluate as true before the main rule body will be processed.
        /// </summary>
        public RulePrecondition If { get; set; }

        public RuleCondition Body { get; set; }

        /// <summary>
        /// Other deployments that must completed successfully before calling this rule.
        /// </summary>
        public string[] DependsOn { get; set; }

        /// <summary>
        /// One or more tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        public TagSet Tag { get; set; }
    }
}
