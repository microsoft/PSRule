// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// The type of resource.
/// </summary>
public enum ResourceKind
{
    /// <summary>
    /// Unknown or empty.
    /// </summary>
    None = 0,

    /// <summary>
    /// A rule resource.
    /// </summary>
    Rule = 1,

    /// <summary>
    /// A baseline resource.
    /// </summary>
    Baseline = 2,

    /// <summary>
    /// A module configuration resource.
    /// </summary>
    ModuleConfig = 3,

    /// <summary>
    /// A selector resource.
    /// </summary>
    Selector = 4,

    /// <summary>
    /// A convention.
    /// </summary>
    Convention = 5,

    /// <summary>
    /// A suppression group.
    /// </summary>
    SuppressionGroup = 6
}
