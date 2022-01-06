// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Badges
{
    public enum BadgeType
    {
        Unknown = 0,

        Success = 1,

        Failure = 2
    }

    public interface IBadge
    {
        /// <summary>
        /// Get the badge as SVG text content.
        /// </summary>
        string ToSvg();

        /// <summary>
        /// Write the SVG badge content directly to disk.
        /// </summary>
        void ToFile(string path);
    }

    public interface IBadgeBuilder
    {
        /// <summary>
        /// Create a badge for the worst case of an analyzed object.
        /// </summary>
        /// <param name="result">A single result. The worst case for all records of an object is used for the badge.</param>
        /// <returns>An instance of a badge.</returns>
        IBadge Create(InvokeResult result);

        /// <summary>
        /// Create a badge for the worst case of all analyzed objects.
        /// </summary>
        /// <param name="result">A enumeration of results. The worst case from all results is used for the badge.</param>
        /// <returns>An instance of a badge.</returns>
        IBadge Create(IEnumerable<InvokeResult> result);

        /// <summary>
        /// Create a custom badge.
        /// </summary>
        /// <param name="title">The left badge text.</param>
        /// <param name="type">Determines if the result is Unknown, Success, or Failure.</param>
        /// <param name="label">The right badge text.</param>
        /// <returns>An instance of a badge.</returns>
        IBadge Create(string title, BadgeType type, string label);
    }

    internal sealed class Badge : IBadge
    {
        private readonly string _LeftText;
        private readonly string _RightText;
        private readonly double _LeftWidth;
        private readonly double _RightWidth;
        private readonly int _MidPadding;
        private readonly int _BorderPadding;
        private readonly string _Fill;

        internal Badge(string left, string right, string fill)
        {
            _LeftWidth = BadgeResources.Measure(left);
            _RightWidth = BadgeResources.Measure(right);

            _LeftText = left;
            _RightText = right;
            _MidPadding = 3;
            _BorderPadding = 7;
            _Fill = fill;
        }

        public override string ToString()
        {
            return ToSvg();
        }

        public string ToSvg()
        {
            var w = (int)Math.Round(_LeftWidth + _RightWidth + 2 * _BorderPadding + 2 * _MidPadding);
            var x = (int)Math.Round(_LeftWidth + _BorderPadding + _MidPadding);

            var builder = new SvgBuilder(
                width: w,
                height: 20,
                textScale: 10,
                midPoint: x,
                rounding: 2,
                borderPadding: _BorderPadding,
                midPadding: _MidPadding);
            builder.Begin(string.Concat(_LeftText, ": ", _RightText));
            builder.Backfill(_Fill);
            builder.TextBlock(_LeftText, _RightText, 110);
            builder.End();
            return builder.ToString();
        }

        public void ToFile(string path)
        {
            path = PSRuleOption.GetRootedPath(path);
            var parentPath = Directory.GetParent(path);
            if (!parentPath.Exists)
                Directory.CreateDirectory(path: parentPath.FullName);

            File.WriteAllText(path, contents: ToSvg());
        }
    }

    internal sealed class BadgeBuilder : IBadgeBuilder
    {
        private const string BADGE_FILL_GREEN = "#4CAF50";
        private const string BADGE_FILL_RED = "#E91E63";
        private const string BADGE_FILL_GREY = "#9E9E9E";

        #region IBadgeBuilder

        public IBadge Create(string title, BadgeType type, string label)
        {
            return CreateCustom(title, label, GetTypeFill(type));
        }

        public IBadge Create(InvokeResult result)
        {
            return CreateInternal(GetOutcome(result));
        }

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

        private static string GetOutcomeFill(RuleOutcome outcome)
        {
            if (outcome == RuleOutcome.Pass)
                return BADGE_FILL_GREEN;

            if (outcome == RuleOutcome.Fail)
                return BADGE_FILL_RED;

            if (outcome == RuleOutcome.Error)
                return BADGE_FILL_RED;

            return BADGE_FILL_GREY;
        }

        private static string GetOutcomeLabel(RuleOutcome outcome)
        {
            if (outcome == RuleOutcome.Pass)
                return PSRuleResources.OutcomePass;

            if (outcome == RuleOutcome.Fail)
                return PSRuleResources.OutcomeFail;

            if (outcome == RuleOutcome.Error)
                return PSRuleResources.OutcomeError;

            return PSRuleResources.OutcomeUnknown;
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
    }
}
