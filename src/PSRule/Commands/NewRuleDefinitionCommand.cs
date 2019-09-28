using PSRule.Parser;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections;
using System.IO;
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
        private const string InvokeBlockCmdlet_TypeParameter = "Type";
        private const string InvokeBlockCmdlet_IfParameter = "If";
        private const string InvokeBlockCmdlet_BodyParameter = "Body";

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty()]
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
        /// An optional preconditions before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] Type { get; set; }

        /// <summary>
        /// Deployments that this deployment depends on.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty()]
        public string[] DependsOn { get; set; }

        /// <summary>
        /// A set of default configuration values.
        /// </summary>
        [Parameter(Mandatory = false)]
        public Hashtable Configure { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsScriptScope())
                throw new RuleRuntimeException(string.Format(PSRuleResources.KeywordScriptScope, LanguageKeywords.Rule));

            var context = PipelineContext.CurrentThread;
            var metadata = GetMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            var tag = GetTag(Tag);
            var source = context.Source.File;

            context.VerboseFoundRule(ruleName: Name, scriptName: MyInvocation.ScriptName);

            CheckDependsOn();

            var ps = PowerShell.Create();
            ps.Runspace = context.GetRunspace();
            ps.AddCommand(new CmdletInfo(InvokeBlockCmdletName, typeof(InvokeRuleBlockCommand)));
            ps.AddParameter(InvokeBlockCmdlet_TypeParameter, Type);
            ps.AddParameter(InvokeBlockCmdlet_IfParameter, If);
            ps.AddParameter(InvokeBlockCmdlet_BodyParameter, Body);

            PipelineContext.EnableLogging(ps);

            var helpInfo = GetHelpInfo(context: context, name: Name) ?? new RuleHelpInfo(name: Name, displayName: Name, moduleName: source.ModuleName);

            if (helpInfo.Synopsis == null)
                helpInfo.Synopsis = metadata.Synopsis;

            var block = new RuleBlock(
                source: source,
                ruleName: Name,
                info: helpInfo,
                condition: ps,
                tag: tag,
                dependsOn: RuleHelper.ExpandRuleName(DependsOn, MyInvocation.ScriptName, source.ModuleName),
                configuration: Configure
            );
            WriteObject(block);
        }

        private void CheckDependsOn()
        {
            if (MyInvocation.BoundParameters.ContainsKey(nameof(DependsOn)) && (DependsOn == null || DependsOn.Length == 0))
            {
                WriteError(new ErrorRecord(
                    exception: new ArgumentNullException(paramName: nameof(DependsOn)),
                    errorId: "PSRule.Runtime.ArgumentNull",
                    errorCategory: ErrorCategory.InvalidArgument,
                    targetObject: null
                ));
            }
        }

        private RuleHelpInfo GetHelpInfo(PipelineContext context, string name)
        {
            if (string.IsNullOrEmpty(context.Source.File.HelpPath))
                return null;

            var culture = context.Culture;

            for (var i = 0; i < culture.Length; i++)
            {
                var path = Path.Combine(context.Source.File.HelpPath, string.Concat(culture[i], "/", name, ".md"));

                if (!File.Exists(path))
                    continue;

                var reader = new MarkdownReader(yamlHeaderOnly: false);
                var stream = reader.Read(markdown: File.ReadAllText(path: path), path: path);
                var lexer = new RuleLexer();
                var document = lexer.Process(stream: stream);

                if (document != null)
                {
                    return new RuleHelpInfo(name: name, displayName: document.Name ?? name, moduleName: context.Source.File.ModuleName)
                    {
                        Synopsis = document.Synopsis?.Text,
                        Description = document.Description?.Text,
                        Recommendation = document.Recommendation?.Text ?? document.Synopsis?.Text,
                        Notes = document.Notes?.Text,
                        Annotations = document.Annotations?.ToHashtable()
                    };
                }
            }
            return null;
        }
    }
}
