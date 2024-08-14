// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Rules;

/// <summary>
/// A rule resource V1.
/// </summary>
public interface IRuleV1 : IResource, IDependencyTarget
{
    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    SeverityLevel Level { get; }

    /// <summary>
    /// A recommendation for the rule.
    /// </summary>
    InfoString Recommendation { get; }

    /// <summary>
    /// A short description of the rule.
    /// </summary>
    string Synopsis { get; }

    /// <summary>
    /// Any additional tags assigned to the rule.
    /// </summary>
    IResourceTags Tag { get; }
}
