// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Badges;

/// <summary>
/// The type of badge.
/// </summary>
public enum BadgeType
{
    /// <summary>
    /// A badge that reports an unknown state.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A badge reporting a successful state.
    /// </summary>
    Success = 1,

    /// <summary>
    /// A badge reporting a failed state.
    /// </summary>
    Failure = 2
}
