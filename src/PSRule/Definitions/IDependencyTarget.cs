// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// An object that relies on a dependency chain.
/// </summary>
public interface IDependencyTarget
{
    /// <summary>
    /// The unique identifier of the resource.
    /// </summary>
    ResourceId Id { get; }

    /// <summary>
    /// A unique reference for the resource.
    /// </summary>
    ResourceId? Ref { get; }

    /// <summary>
    /// Additional aliases for the resource.
    /// </summary>
    ResourceId[]? Alias { get; }

    /// <summary>
    /// Resources this target depends on.
    /// </summary>
    ResourceId[]? DependsOn { get; }

    /// <summary>
    /// Determines if the source was imported as a dependency.
    /// </summary>
    bool Dependency { get; }
}
