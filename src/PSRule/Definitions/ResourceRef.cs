// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal class ResourceRef(string id, ResourceKind kind) : IEquatable<ResourceRef>
{
    public readonly string Id = id ?? throw new ArgumentNullException(nameof(id));
    public readonly ResourceKind Kind = kind;

    public bool Equals(ResourceRef? other)
    {
        return other is not null && Id == other.Id && Kind == other.Kind;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceRef r && Equals(r);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + Kind.GetHashCode();
            hash = hash * 23 + Id.GetHashCode();
            return hash;
        }
    }
}
