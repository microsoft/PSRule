// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Input for the upgrade dependency command handler.
/// </summary>
public sealed class UpgradeDependencyCommandHandlerInput
{
    /// <summary>
    /// The path to the options file.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The module to upgrade.
    /// </summary>
    public string? Module { get; set; }
}
