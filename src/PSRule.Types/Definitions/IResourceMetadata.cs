// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Additional resource metadata.
/// </summary>
public interface IResourceMetadata
{
    /// <summary>
    /// Annotations on the resource.
    /// </summary>
    public IResourceAnnotations Annotations { get; }
}
