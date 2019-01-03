using PSRule.Host;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace PSRule.Rules
{
    public delegate bool RulePrecondition();

    public delegate RuleConditionResult RuleCondition();

    /// <summary>
    /// Define an instance of a rule block. Each rule block has a unique id.
    /// </summary>
    [DebuggerDisplay("{RuleId} @{SourcePath}")]
    public sealed class RuleBlock : ILanguageBlock, IDependencyTarget
    {
        public RuleBlock(string sourcePath, string ruleName, string description)
        {
            SourcePath = sourcePath;
            RuleName = ruleName;

            var scriptFileName = Path.GetFileName(sourcePath);
            RuleId = string.Concat(scriptFileName, '/', ruleName);

            Description = description;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        public string RuleId { get; private set; }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public string RuleName { get; private set; }

        /// <summary>
        /// The script file path where the rule is defined.
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A pre-condition that if set, must evaluate as true before the main rule body will be processed.
        /// </summary>
        public ScriptBlock If { get; set; }

        /// <summary>
        /// The body of the rule definition where conditions are provided that either pass or fail the rule.
        /// </summary>
        public PowerShell Body { get; set; }

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
