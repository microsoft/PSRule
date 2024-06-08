// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Badges;

/// <summary>
/// A badge builder that implements the Badge API within PSRule.
/// </summary>
internal sealed class BadgeBuilder : IBadgeBuilder
{
    private const string BADGE_FILL_GREEN = "#4CAF50";
    private const string BADGE_FILL_RED = "#E91E63";
    private const string BADGE_FILL_GREY = "#9E9E9E";

    #region IBadgeBuilder

    /// <inheritdoc/>
    public IBadge Create(string title, BadgeType type, string label)
    {
        return CreateCustom(title, label, GetTypeFill(type));
    }

    /// <inheritdoc/>
    public IBadge Create(InvokeResult result)
    {
        return CreateInternal(GetOutcome(result));
    }

    /// <inheritdoc/>
    public IBadge Create(IEnumerable<InvokeResult> result)
    {
        var worstCase = RuleOutcome.Pass;
        var i = 0;
        if (result != null)
        {
            foreach (var r in result)
            {
                if (!r.IsSuccess())
                    worstCase = worstCase.GetWorstCase(r.Outcome);

                i++;
            }
        }
        return CreateInternal(i == 0 ? RuleOutcome.None : worstCase);
    }

    #endregion IBadgeBuilder

    #region Private helper methods

    private static string GetOutcomeFill(RuleOutcome outcome)
    {
        if (outcome == RuleOutcome.Pass)
            return BADGE_FILL_GREEN;

        if (outcome == RuleOutcome.Fail)
            return BADGE_FILL_RED;

        return outcome == RuleOutcome.Error ? BADGE_FILL_RED : BADGE_FILL_GREY;
    }

    private static string GetOutcomeLabel(RuleOutcome outcome)
    {
        if (outcome == RuleOutcome.Pass)
            return PSRuleResources.OutcomePass;

        if (outcome == RuleOutcome.Fail)
            return PSRuleResources.OutcomeFail;

        return outcome == RuleOutcome.Error ? PSRuleResources.OutcomeError : PSRuleResources.OutcomeUnknown;
    }

    private static RuleOutcome GetOutcome(InvokeResult result)
    {
        return result == null || !result.IsProcessed() ? RuleOutcome.None : result.Outcome;
    }

    private static string GetTypeFill(BadgeType type)
    {
        if (type == BadgeType.Success)
            return BADGE_FILL_GREEN;

        if (type == BadgeType.Failure)
            return BADGE_FILL_RED;

        return BADGE_FILL_GREY;
    }

    private static IBadge CreateCustom(string title, string label, string fill)
    {
        return new Badge(title, label, fill);
    }

    private static IBadge CreateInternal(RuleOutcome outcome)
    {
        var outcomeLabel = GetOutcomeLabel(outcome);
        var outcomeFill = GetOutcomeFill(outcome);
        return CreateCustom("PSRule", outcomeLabel, outcomeFill);
    }

    #endregion Private helper methods
}
