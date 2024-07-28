// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Rules;

/// <summary>
/// A specification for a rule resource.
/// </summary>
internal interface IRuleSpec
{
    /// <summary>
    /// The of the rule condition that will be evaluated.
    /// </summary>
    LanguageIf Condition { get; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    SeverityLevel? Level { get; }

    /// <summary>
    /// An optional type pre-condition before the rule is evaluated.
    /// </summary>
    string[] Type { get; }

    /// <summary>
    /// An optional selector pre-condition before the rule is evaluated.
    /// </summary>
    string[] With { get; }

    /// <summary>
    /// An optional sub-selector pre-condition before the rule is evaluated.
    /// </summary>
    LanguageIf Where { get; }
}
