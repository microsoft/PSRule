// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Data;

/// <summary>
/// A target object that is processed by PSRule.
/// This wraps a objects with additional metadata about the object.
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
    /// The name of the object.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The type of the object.
    /// </summary>
    string? Type { get; }

    /// <summary>
    /// The path of the object.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// The scopes associated with the object.
    /// </summary>
    string[]? Scope { get; }

    /// <summary>
    /// The value of the object.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// The data associated with the object.
    /// </summary>
    Hashtable? GetData();
}
