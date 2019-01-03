using PSRule.Host;
using System;
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
    public sealed class RuleBlock : ILanguageBlock, IDependencyTarget, IDisposable
    {
        public RuleBlock(string sourcePath, string ruleName, string description, PowerShell condition, TagSet tag, string[] dependsOn)
        {
            SourcePath = sourcePath;
            RuleName = ruleName;

            var scriptFileName = Path.GetFileName(sourcePath);
            RuleId = string.Concat(scriptFileName, '/', ruleName);

            Description = description;
            Condition = condition;
            Tag = tag;
            DependsOn = dependsOn;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        public readonly string RuleId;

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public readonly string RuleName;

        /// <summary>
        /// The script file path where the rule is defined.
        /// </summary>
        public readonly string SourcePath;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// The body of the rule definition where conditions are provided that either pass or fail the rule.
        /// </summary>
        public readonly PowerShell Condition;

        /// <summary>
        /// Other deployments that must completed successfully before calling this rule.
        /// </summary>
        public readonly string[] DependsOn;

        /// <summary>
        /// One or more tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        public readonly TagSet Tag;

        string ILanguageBlock.SourcePath => SourcePath;

        string IDependencyTarget.RuleId => RuleId;

        string[] IDependencyTarget.DependsOn => DependsOn;

        #region IDisposable

        public void Dispose()
        {
            Condition.Dispose();
        }

        #endregion IDisposable
    }
}
