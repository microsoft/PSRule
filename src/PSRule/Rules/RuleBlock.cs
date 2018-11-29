using PSRule.Host;

namespace PSRule.Rules
{
    public delegate bool RulePrecondition();

    public delegate RuleConditionResult RuleCondition();

    /// <summary>
    /// Define an instance of a deployment block. Each deployment block has a unique name.
    /// </summary>
    public sealed class RuleBlock : ILanguageBlock
    {
        public RuleBlock(string name)
        {
            Name = name;
        }

        public string SourcePath { get; set; }

        /// <summary>
        /// The name of the deployment.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the deployment.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A precondition that if set, must evaluate as true before the main rule body will be processed.
        /// </summary>
        public RulePrecondition If { get; set; }

        public RuleCondition Body { get; set; }

        /// <summary>
        /// Other deployments that must completed successfully before calling this deployment.
        /// </summary>
        public string[] DependsOn { get; set; }

        public TagSet Tag { get; set; }
    }
}
