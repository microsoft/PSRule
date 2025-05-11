// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// A specification for a V2 suppression group resource.
/// </summary>
internal interface ISuppressionGroupV2Spec : ISuppressionGroupSpec
{
    /// <summary>
    /// The date time that the suppression is valid until.
    /// After this date time, the suppression is ignored.
    /// When not set, the suppression does not expire.
    /// This RFC3339 (ISO 8601) formatted date time using the format yyyy-MM-ddTHH:mm:ssZ.
    /// </summary>
    DateTime? ExpiresOn { get; set; }

    /// <summary>
    /// This only applies to rules that match the specified rule names.
    /// </summary>
    string[]? Rule { get; }

    /// <summary>
    /// An optional type pre-condition before the suppression group is evaluated.
    /// </summary>
    string[]? Type { get; }

    /// <summary>
    /// An expression. If the expression evaluates as true and rules specified by <see cref="Rule"/> are suppressed.
    /// </summary>
    LanguageIf? If { get; }
}
