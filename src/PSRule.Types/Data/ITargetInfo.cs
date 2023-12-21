// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// An interface implemented by objects that automatically provide binding and source information.
/// </summary>
internal interface ITargetInfo
{
    /// <summary>
    /// The target name provided by the object.
    /// </summary>
    string? TargetName { get; }

    /// <summary>
    /// The target type provided by the object.
    /// </summary>
    string? TargetType { get; }

    /// <summary>
    /// The source information provided by the object.
    /// </summary>
    TargetSourceInfo Source { get; }
}
