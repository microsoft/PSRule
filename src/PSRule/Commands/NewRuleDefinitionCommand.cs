using PSRule.Pipeline;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// A Rule language block.
    /// </summary>
    [Cmdlet(VerbsCommon.New, RuleLanguageNouns.RuleDefinition)]
    internal sealed class NewRuleDefinitionCommand : LanguageBlock
    {
        private const string InvokeBlockCmdletName = "Invoke-RuleBlock";
        private const string InvokeBlockCmdlet_IfParameter = "If";
        private const string InvokeBlockCmdlet_BodyParameter = "Body";

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The definition of the deployment.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public ScriptBlock Body { get; set; }

        /// <summary>
        /// A set of tags with additional metadata for the rule.
        /// </summary>
        [Parameter(Mandatory = false)]
        public Hashtable Tag { get; set; }

        /// <summary>
        /// An optional precondition before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock If { get; set; }

        /// <summary>
        /// Deployments that this deployment depends on.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] DependsOn { get; set; }

        /// <summary>
        /// A set of default configuration values.
        /// </summary>
        [Parameter(Mandatory = false)]
        public Hashtable Configure { get; set; }

        protected override void ProcessRecord()
        {
            var metadata = GetMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            var tag = GetTag(Tag);
            var moduleName = PipelineContext.CurrentThread.ModuleName;

            PipelineContext.CurrentThread.VerboseFoundRule(ruleName: Name, scriptName: MyInvocation.ScriptName);

            var ps = PowerShell.Create();
            ps.Runspace = PipelineContext.CurrentThread.GetRunspace();
            ps.AddCommand(new CmdletInfo(InvokeBlockCmdletName, typeof(InvokeRuleBlockCommand)));
            ps.AddParameter(InvokeBlockCmdlet_IfParameter, If);
            ps.AddParameter(InvokeBlockCmdlet_BodyParameter, Body);

            PipelineContext.EnableLogging(ps);

            var block = new RuleBlock(
                sourcePath: MyInvocation.ScriptName,
                moduleName: moduleName,
                ruleName: Name,
                description: metadata.Description,
                condition: ps,
                tag: tag,
                dependsOn: RuleHelper.ExpandRuleName(DependsOn, MyInvocation.ScriptName, moduleName),
                configuration: Configure
            );

            WriteObject(block);
        }
    }
}
