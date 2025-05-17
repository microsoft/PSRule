// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// The AnyOf keyword.
/// </summary>
[Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AnyOf)]
internal sealed class AssertAnyOfCommand : RuleKeyword
{
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock? Body { get; set; }

    protected override void ProcessRecord()
    {
        if (!IsConditionScope())
            throw ConditionScopeException(LanguageKeywords.AnyOf);

        var invokeResult = RuleConditionHelper.Create(Body.Invoke());
        var result = invokeResult.AnyOf();

        LegacyRunspaceContext.CurrentThread.VerboseConditionResult(
            condition: RuleLanguageNouns.AnyOf,
            pass: invokeResult.Pass,
            count: invokeResult.Count,
            outcome: result);
        WriteObject(result);
    }
}
