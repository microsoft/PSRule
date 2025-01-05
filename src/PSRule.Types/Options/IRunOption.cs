// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace PSRule.Options;

/// <summary>
/// Options that configure runs.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IRunOption : IOption
{
    /// <summary>
    /// Configures the run category that is used as an identifier for output results.
    /// </summary>
    string? Category { get; }

    /// <summary>
    /// Configure the run description that is displayed in output.
    /// </summary>
    string? Description { get; }
}
