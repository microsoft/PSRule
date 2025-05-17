// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// The AllOf keyword.
/// </summary>
[Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AllOf)]
internal sealed class AssertAllOfCommand : RuleKeyword
{
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock? Body { get; set; }

    protected override void ProcessRecord()
    {
        if (!IsConditionScope())
            throw ConditionScopeException(LanguageKeywords.AllOf);

        var invokeResult = RuleConditionHelper.Create(Body.Invoke());
        var result = invokeResult.AllOf();

        LegacyRunspaceContext.CurrentThread.VerboseConditionResult(
            condition: RuleLanguageNouns.AllOf,
            pass: invokeResult.Pass,
            count: invokeResult.Count,
            outcome: result);
        WriteObject(result);
    }
}
