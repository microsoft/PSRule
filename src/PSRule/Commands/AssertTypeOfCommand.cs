// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// The TypeOf keyword.
/// </summary>
[Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.TypeOf)]
internal sealed class AssertTypeOfCommand : RuleKeyword
{
    [Parameter(Mandatory = true, Position = 0)]
    public string[] TypeName { get; set; }

    [Parameter(Mandatory = false)]
    public string Reason { get; set; }

    [Parameter(Mandatory = false, ValueFromPipeline = true)]
    public PSObject InputObject { get; set; }

    protected override void ProcessRecord()
    {
        if (!IsRuleScope())
            throw RuleScopeException(LanguageKeywords.TypeOf);

        var inputObject = InputObject ?? GetTargetObjectValue();
        var result = false;

        if (inputObject != null)
        {
            var actualTypeNames = PSObject.AsPSObject(inputObject).TypeNames.ToArray();
            result = (actualTypeNames.Intersect(TypeName).Any());
        }

        LegacyRunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.TypeOf, outcome: result);
        if (!(result || TryReason(null, Reason, null)))
        {
            WriteReason(
                path: null,
                text: ReasonStrings.TypeOf,
                args: string.Join(", ", TypeName)
            );
        }
        WriteObject(result);
    }
}
