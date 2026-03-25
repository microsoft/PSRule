// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A node in a dependency graph.
/// </summary>
/// <typeparam name="T">The type of the value contained in the node.</typeparam>
public interface IDependencyNode<T>
{
    /// <summary>
    /// The value of the node.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Indicates if the node was skipped.
    /// </summary>
    bool Skipped { get; }

    /// <summary>
    /// Mark the node as passed.
    /// </summary>
    void Pass();

    /// <summary>
    /// Mark the node as failed.
    /// </summary>
    void Fail();
}
