// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// An instance of a target object.
/// </summary>
public interface ITargetObject
{
    /// <summary>
    /// Sources for the target object.
    /// </summary>
    IEnumerable<TargetSourceInfo> Source { get; }

    /// <summary>
    /// The target name of the object.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The target type of the object.
    /// </summary>
    string? Type { get; }

    /// <summary>
    /// The path of the object.
    /// </summary>
    string? Path { get; }
}
