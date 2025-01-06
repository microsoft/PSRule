// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Detailed information about the rule result.
/// </summary>
public interface IResultDetail
{
    /// <summary>
    /// Any reasons for the result.
    /// </summary>
    IEnumerable<IResultReason> Reason { get; }
}
