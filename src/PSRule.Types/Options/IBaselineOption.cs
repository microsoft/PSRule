// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Options;

/// <summary>
/// Options that configure baselines.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IBaselineOption : IOption
{
    /// <summary>
    /// A mapping of baseline group names to baselines.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#baselinegroup"/>.
    /// </remarks>
    StringArrayMap? Group { get; }
}
