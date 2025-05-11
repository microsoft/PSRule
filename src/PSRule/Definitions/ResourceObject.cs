// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions;

#nullable enable

/// <summary>
/// A resource object.
/// </summary>
[DebuggerDisplay("{Block.Kind} - {Block.Id.Value}")]
public sealed class ResourceObject(IResource? block, string? apiVersion, string? kind)
{
    /// <summary>
    /// The resource block.
    /// </summary>
    public IResource? Block { get; } = block;

    /// <summary>
    /// The API version of the resource.
    /// </summary>
    public string? ApiVersion { get; } = apiVersion;

    /// <summary>
    /// The kind of the resource.
    /// </summary>
    public string? Kind { get; } = kind;

    internal bool Visit(IResourceVisitor visitor)
    {
        return Block != null && visitor != null && visitor.Visit(Block);
    }
}

#nullable restore
