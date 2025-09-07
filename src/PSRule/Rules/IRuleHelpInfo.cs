// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions;

namespace PSRule.Rules;

/// <summary>
/// A rule help information structure.
/// </summary>
public interface IRuleHelpInfo : IResourceHelpInfo
{
    /// <summary>
    /// The rule recommendation.
    /// </summary>
    InfoString Recommendation { get; }

    /// <summary>
    /// Additional annotations, which are string key/ value pairs.
    /// </summary>
    Hashtable Annotations { get; }

    /// <summary>
    /// The name of the module where the rule was loaded from.
    /// </summary>
    string? ModuleName { get; }

    /// <summary>
    /// Additional online links to reference information for the rule.
    /// </summary>
    Link[] Links { get; }
}
