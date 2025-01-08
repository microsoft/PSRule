// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions;

#nullable enable

/// <summary>
/// A resource object.
/// </summary>
[DebuggerDisplay("{Block.Kind} - {Block.Id.Value}")]
public sealed class ResourceObject
{
    internal ResourceObject(IResource? block, string? apiVersion, string? kind)
    {
        Block = block;
        ApiVersion = apiVersion;
        Kind = kind;
    }

    /// <summary>
    /// The resource block.
    /// </summary>
    public IResource? Block { get; }

    /// <summary>
    /// The API version of the resource.
    /// </summary>
    public string? ApiVersion { get; }

    /// <summary>
    /// The kind of the resource.
    /// </summary>
    public string? Kind { get; }

    internal bool Visit(IResourceVisitor visitor)
    {
        return Block != null && visitor != null && visitor.Visit(Block);
    }
}

#nullable restore
