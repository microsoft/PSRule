// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Definitions.Conventions;
using PSRule.Runtime;

namespace PSRule.Commands;

#nullable enable

[Cmdlet(VerbsData.Export, RuleLanguageNouns.Convention)]
internal sealed class ExportConventionCommand : LanguageBlock
{
    private const string CmdletName = "Invoke-PSRuleConvention";
    private const string Cmdlet_IfParameter = "If";
    private const string Cmdlet_BodyParameter = "Body";
    private const string Cmdlet_ScopeParameter = "Scope";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(3, 128)]

    public string Name { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// A script block to call once before any objects are processed.
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateNotNullOrEmpty()]
    public ScriptBlock? Initialize { get; set; }

    /// <summary>
    /// A script block to call once per object before being processed by any rule.
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateNotNullOrEmpty()]
    public ScriptBlock? Begin { get; set; }

    /// <summary>
    /// A script block to call once per object after rules are processed.
    /// </summary>
    [Parameter(Mandatory = false, Position = 1)]
    [ValidateNotNullOrEmpty()]
    public ScriptBlock? Process { get; set; }

    /// <summary>
    /// A script block to call once after all rules and all objects are processed.
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateNotNullOrEmpty()]
    public ScriptBlock? End { get; set; }

    /// <summary>
    /// An optional pre-condition before the convention is evaluated.
    /// </summary>
    [Parameter(Mandatory = false)]
    public ScriptBlock? If { get; set; }

    protected override void ProcessRecord()
    {
        //if (!IsScriptScope())
        //    throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordScriptScope, LanguageKeywords.Rule));

        var context = LegacyRunspaceContext.CurrentThread;
        if (context == null) return;

        var source = context.Source!;
        var errorPreference = GetErrorActionPreference();
        var commentMetadata = GetCommentMetadata(source, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
        var metadata = new ResourceMetadata
        {
            Name = Name
        };
        var extent = new SourceExtent(
            file: source,
            line: MyInvocation.ScriptLineNumber,
            position: MyInvocation.OffsetInLine
        );

        context.VerboseFoundResource(name: Name, moduleName: source.Module, scriptName: MyInvocation.ScriptName);

        var helpInfo = new ResourceHelpInfo(Name, Name, new InfoString(commentMetadata.Synopsis), new InfoString());

#pragma warning disable CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
        var block = new ScriptBlockConvention(
            source: source,
            metadata: metadata,
            info: helpInfo,
            initialize: ConventionBlock(context, Initialize, RunspaceScope.ConventionInitialize),
            begin: ConventionBlock(context, Begin, RunspaceScope.ConventionBegin),
            process: ConventionBlock(context, Process, RunspaceScope.ConventionProcess),
            end: ConventionBlock(context, End, RunspaceScope.ConventionEnd),
            errorPreference: errorPreference,
            flags: ResourceFlags.None,
            extent: extent
        );
#pragma warning restore CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
        WriteObject(block);
    }

    private LanguageScriptBlock? ConventionBlock(LegacyRunspaceContext context, ScriptBlock? block, RunspaceScope scope)
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

#nullable restore
