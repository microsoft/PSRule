// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Detailed information about the rule result.
/// </summary>
public interface IResultDetailV2
{
    /// <summary>
    /// Any reasons for the result.
    /// </summary>
    IEnumerable<IResultReasonV2> Reason { get; }
}
