using PSRule.Host;
using System;
using System.Collections;
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
        internal RuleBlock(RuleSource source, string ruleName, RuleHelpInfo info, PowerShell condition, TagSet tag, string[] dependsOn, Hashtable configuration)
        {
            Source = source;
            RuleName = ruleName;

            var scriptFileName = Path.GetFileName(Source.Path);

            // Get either scriptFileName/RuleName or Module/scriptFileName/RuleName
            RuleId = (Source.ModuleName == null) ?
                string.Concat(scriptFileName, '/', RuleName) : string.Concat(Source.ModuleName, '/', scriptFileName, '/', RuleName);

            Info = info;
            Condition = condition;
            Tag = tag;
            DependsOn = dependsOn;
            Configuration = configuration;
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
        //public readonly string SourcePath;

        /// <summary>
        /// The name of the module where the rule is defined, or null if the rule is not defined in a module.
        /// </summary>
        //public readonly string ModuleName;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        //public readonly string Synopsis;

        /// <summary>
        /// A human readable block of text that identifies the recommendation to address the rule when failed.
        /// </summary>
        //public readonly string Recommendation;

        /// <summary>
        /// The body of the rule definition where conditions are provided that either pass or fail the rule.
        /// </summary>
        public readonly PowerShell Condition;

        /// <summary>
        /// Other deployments that must completed successfully before calling this rule.
        /// </summary>
        public readonly string[] DependsOn;

        /// <summary>
        /// Tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        public readonly TagSet Tag;

        //public readonly TagSet Annotations;

        /// <summary>
        /// Configuration defaults for the rule definition.
        /// </summary>
        /// <remarks>
        /// These defaults are used when the value does not exist in the baseline configuration.
        /// </remarks>
        public readonly Hashtable Configuration;

        // Alias to synopsis
        //public string Description => Synopsis;

        public readonly RuleHelpInfo Info;

        public readonly RuleSource Source;

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

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
