// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A collection of sources for a target object.
/// </summary>
public interface ITargetSourceCollection
{
    /// <summary>
    /// Get the source details by source type.
    /// </summary>
    TargetSourceInfo this[string type] { get; }
}
