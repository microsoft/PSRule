// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Badges;

/// <summary>
/// A builder for the badge API.
/// </summary>
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
