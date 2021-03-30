// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Parser;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// A Rule language block.
    /// </summary>
    [Cmdlet(VerbsCommon.New, RuleLanguageNouns.RuleDefinition)]
    internal sealed class NewRuleDefinitionCommand : LanguageBlock
    {
        private const string CmdletName = "Invoke-RuleBlock";
        private const string Cmdlet_TypeParameter = "Type";
        private const string Cmdlet_IfParameter = "If";
        private const string Cmdlet_WithParameter = "With";
        private const string Cmdlet_BodyParameter = "Body";

        private const string Markdown_Extension = ".md";

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
        /// An optional script precondition before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock If { get; set; }

        /// <summary>
        /// An optional type precondition before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] Type { get; set; }

        /// <summary>
        /// An optional selector precondition before the rule is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] With { get; set; }

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
            if (!IsSourceScope())
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordSourceScope, LanguageKeywords.Rule));

            var context = RunspaceContext.CurrentThread;
            var errorPreference = GetErrorActionPreference();
            var metadata = GetCommentMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            var tag = GetTag(Tag);
            var source = context.Source.File;
            var extent = new RuleExtent(
                file: source.Path,
                startLineNumber: Body.Ast.Extent.StartLineNumber
            );

            context.VerboseFoundResource(name: Name, moduleName: source.ModuleName, scriptName: MyInvocation.ScriptName);

            CheckDependsOn();
            var ps = GetCondition(context);
            var helpInfo = GetHelpInfo(context: context, name: Name) ?? new RuleHelpInfo(
                name: Name,
                displayName: Name,
                moduleName: source.ModuleName
            );

            if (helpInfo.Synopsis == null)
                helpInfo.Synopsis = metadata.Synopsis;

#pragma warning disable CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
            var block = new RuleBlock(
                source: source,
                ruleName: Name,
                info: helpInfo,
                condition: ps,
                tag: tag,
                dependsOn: RuleHelper.ExpandRuleName(DependsOn, MyInvocation.ScriptName, source.ModuleName),
                configuration: Configure,
                extent: extent,
                errorPreference: errorPreference
            );
#pragma warning restore CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
            WriteObject(block);
        }

        private PowerShell GetCondition(RunspaceContext context)
        {
            var result = context.GetPowerShell();
            result.AddCommand(new CmdletInfo(CmdletName, typeof(InvokeRuleBlockCommand)));
            result.AddParameter(Cmdlet_TypeParameter, Type);
            result.AddParameter(Cmdlet_WithParameter, With);
            result.AddParameter(Cmdlet_IfParameter, If);
            result.AddParameter(Cmdlet_BodyParameter, Body);
            return result;
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

        private static RuleHelpInfo GetHelpInfo(RunspaceContext context, string name)
        {
            if (string.IsNullOrEmpty(context.Source.File.HelpPath))
                return null;

            var helpFileName = string.Concat(name, Markdown_Extension);
            var path = context.GetLocalizedPath(helpFileName);
            if (path == null || !TryDocument(path, out RuleDocument document))
                return null;

            return new RuleHelpInfo(name: name, displayName: document.Name ?? name, moduleName: context.Source.File.ModuleName)
            {
                Synopsis = document.Synopsis?.Text,
                Description = document.Description?.Text,
                Recommendation = document.Recommendation?.Text ?? document.Synopsis?.Text,
                Notes = document.Notes?.Text,
                Links = GetLinks(document.Links),
                Annotations = document.Annotations?.ToHashtable()
            };
        }

        private static bool TryDocument(string path, out RuleDocument document)
        {
            document = null;
            var markdown = File.ReadAllText(path);
            if (string.IsNullOrEmpty(markdown))
                return false;

            var reader = new MarkdownReader(yamlHeaderOnly: false);
            var stream = reader.Read(markdown, path);
            var lexer = new RuleLexer();
            document = lexer.Process(stream);
            return document != null;
        }

        private static RuleHelpInfo.Link[] GetLinks(Link[] links)
        {
            if (links == null || links.Length == 0)
                return null;

            var result = new RuleHelpInfo.Link[links.Length];
            for (var i = 0; i < links.Length; i++)
                result[i] = new RuleHelpInfo.Link(links[i].Name, links[i].Uri);

            return result;
        }
    }
}
