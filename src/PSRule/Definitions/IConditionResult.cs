// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A result from an language condition.
/// </summary>
public interface IConditionResult
{
    /// <summary>
    /// Determine if the condition had errors.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// The number of sub-conditions that were evaluated.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The number of sub-conditions that passed.
    /// </summary>
    int Pass { get; }
}
