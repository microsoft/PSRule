// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Conventions;
using PSRule.Runtime;
using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsData.Export, RuleLanguageNouns.Convention)]
    internal sealed class ExportConventionCommand : LanguageBlock
    {
        private const string CmdletName = "Invoke-PSRuleConvention";
        private const string Cmdlet_IfParameter = "If";
        private const string Cmdlet_BodyParameter = "Body";
        private const string Cmdlet_ScopeParameter = "Scope";

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty()]
        public ScriptBlock Begin { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        [ValidateNotNullOrEmpty()]
        public ScriptBlock Process { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty()]
        public ScriptBlock End { get; set; }

        /// <summary>
        /// An optional precondition before the hook is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock If { get; set; }

        protected override void ProcessRecord()
        {
            //if (!IsScriptScope())
            //    throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordScriptScope, LanguageKeywords.Rule));

            var context = RunspaceContext.CurrentThread;
            var errorPreference = GetErrorActionPreference();
            var commentMetadata = GetCommentMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            var source = context.Source.File;
            var metadata = new ResourceMetadata
            {
                Name = Name
            };

            context.VerboseFoundResource(name: Name, moduleName: source.ModuleName, scriptName: MyInvocation.ScriptName);

            var helpInfo = new ResourceHelpInfo(
                synopsis: commentMetadata.Synopsis
            );

#pragma warning disable CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
            var block = new ScriptBlockConvention(
                source: source,
                metadata: metadata,
                info: helpInfo,
                begin: ConventionBlock(context, Begin, RunspaceScope.ConventionBegin),
                process: ConventionBlock(context, Process, RunspaceScope.ConventionProcess),
                end: ConventionBlock(context, End, RunspaceScope.ConventionEnd),
                errorPreference: errorPreference
            );
#pragma warning restore CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
            WriteObject(block);
        }

        private LanguageScriptBlock ConventionBlock(RunspaceContext context, ScriptBlock block, RunspaceScope scope)
        {
            if (block == null)
                return null;

            // Create PS instance for execution
            var ps = context.GetPowerShell();
            ps.AddCommand(new CmdletInfo(CmdletName, typeof(InvokeConventionCommand)));
            ps.AddParameter(Cmdlet_IfParameter, If);
            ps.AddParameter(Cmdlet_BodyParameter, block);
            ps.AddParameter(Cmdlet_ScopeParameter, scope);
            return new LanguageScriptBlock(ps);
        }
    }
}
