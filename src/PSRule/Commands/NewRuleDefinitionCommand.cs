using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsCommon.New, RuleLanguageNouns.RuleDefinition)]
    internal sealed class NewRuleDefinitionCommand : LanguageBlockCommand
    {
        /// <summary>
        /// The name of the deployment.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The definition of the deployment.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public RuleCondition Body { get; set; }

        /// <summary>
        /// A set of tags with additional metadata for the rule.
        /// </summary>
        [Parameter(Mandatory = false)]
        public Hashtable Tag { get; set; }

        /// <summary>
        /// An optional precondition before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public RulePrecondition If { get; set; }

        /// <summary>
        /// Deployments that this deployment depends on.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] DependsOn { get; set; }

        protected override void ProcessRecord()
        {
            var metadata = GetMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            var tag = GetTag(Tag);

            WriteVerbose($"[PSRule][R][{Name}]::BEGIN");

            var block = new RuleBlock(Name)
            {
                Body = Body,
                Description = metadata.Description,
                Tag = tag,
                DependsOn = DependsOn,
                If = If
            };

            WriteObject(block);

            WriteVerbose($"[PSRule][R][{Name}]::END");
        }
    }
}
