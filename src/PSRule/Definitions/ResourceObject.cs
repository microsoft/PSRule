// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions;

/// <summary>
/// A resource object.
/// </summary>
[DebuggerDisplay("{Block.Kind} - {Block.Id.Value}")]
public sealed class ResourceObject
{
    internal ResourceObject(IResource block)
    {
        Block = block;
    }

    internal IResource Block { get; }

    internal bool Visit(IResourceVisitor visitor)
    {
        return Block != null && visitor != null && visitor.Visit(Block);
    }
}
