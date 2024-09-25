// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Options;

/// <summary>
/// An interface for getting the current options configured during runtime.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IOptionContext
{
    /// <summary>
    /// Options that configure the execution sandbox.
    /// </summary>
    IExecutionOption Execution { get; }

    /// <summary>
    /// Options that configure baselines.
    /// </summary>
    IBaselineOption Baseline { get; }
}
