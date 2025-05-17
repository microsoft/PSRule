// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A collection of issues reported by a downstream tool.
/// </summary>
public interface ITargetIssueCollection
{
    /// <summary>
    /// Get any issues from the collection that match the specified type.
    /// </summary>
    /// <param name="type">The type of the issue.</param>
    /// <returns>Returns issues that match the specified <paramref name="type"/>.</returns>
    TargetIssueInfo[] Get(string? type = null);

    /// <summary>
    /// Check if the collection contains any of the specified issue type.
    /// </summary>
    /// <param name="type">The type of the issue.</param>
    /// <returns>Returns <c>true</c> if any the collection contains any issues matching the specified <paramref name="type"/>.</returns>
    bool Any(string? type = null);
}
