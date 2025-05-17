// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Commands;

/// <summary>
/// The Recommend keyword.
/// </summary>
[Cmdlet(VerbsCommunications.Write, RuleLanguageNouns.Recommendation)]
internal sealed class WriteRecommendationCommand : RuleKeyword
{
    [Parameter(Mandatory = false, Position = 0)]
    [Alias(aliasNames: "Message")]
    public string? Text { get; set; }

    protected override void ProcessRecord()
    {
        if (!IsConditionScope())
            throw ConditionScopeException(LanguageKeywords.Recommend);

        var result = GetResult();

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Text)) && string.IsNullOrEmpty(result.Info.Recommendation?.Text))
            result.Info.Recommendation.Text = Text;
    }
}
