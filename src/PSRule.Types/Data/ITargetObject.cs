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
    /// A source may for the target object.
    /// </summary>
    /// <remarks>
    /// This is a placeholder for future implementation and should currently be null.
    /// </remarks>
    ITargetSourceMap? SourceMap { get; }

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

    /// <summary>
    /// The value of the object.
    /// </summary>
    object Value { get; }
}
