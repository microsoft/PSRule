// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A resource object.
/// </summary>
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
