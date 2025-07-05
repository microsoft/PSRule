// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Rules;

#pragma warning disable CS8618 // Keyword required is not supported in .NET Standard 2.0

/// <summary>
/// A specification for a V1 rule resource.
/// </summary>
internal sealed class RuleV1Spec : Spec, IRuleSpec
{
    /// <inheritdoc/>
    public LanguageIf Condition { get; set; }

    /// <inheritdoc/>
    public SeverityLevel? Level { get; set; }

    /// <inheritdoc/>
    public string Recommend { get; set; }

    /// <inheritdoc/>
    public string[] Type { get; set; }

    /// <inheritdoc/>
    public string[] With { get; set; }

    /// <inheritdoc/>
    public LanguageIf Where { get; set; }
}

#pragma warning restore CS8618
