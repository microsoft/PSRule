// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Definitions.Rules;

namespace PSRule.Options;

/// <summary>
/// Options that configure additional rule overrides.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IOverrideOption
{
    /// <summary>
    /// A mapping of rule severity levels to override.
    /// </summary>
    EnumMap<SeverityLevel>? Level { get; }
}
