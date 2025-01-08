// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A base interface for a resource with a specification.
/// </summary>
public interface IResource<TSpec> : IResource
    where TSpec : ISpec
{
    /// <summary>
    /// Get the specification for the resource.
    /// </summary>
    TSpec Spec { get; }
}
