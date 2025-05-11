// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal abstract class ResourceRef(string id, ResourceKind kind)
{
    public readonly string Id = id;
    public readonly ResourceKind Kind = kind;
}
