// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// An ID reference to a resource.
/// The reference may be a full resource ID or just a name depending on context.
/// </summary>
public struct ResourceIdReference : IEquatable<ResourceIdReference>, IEquatable<string>, IEquatable<ResourceId>
{
    private readonly int _HashCode;

    /// <summary>
    /// Creates a <see cref="ResourceIdReference"/>.
    /// </summary>
    /// <param name="raw">The literal string representation of the resource identifier.</param>
    /// <param name="scope">The scope of the resource. When null, the scope points to any scope.</param>
    /// <param name="name">A unique name for the resource within the specified <see cref="Scope"/>.</param>
    private ResourceIdReference(string raw, string? scope, string name)
    {
        Scope = scope;
        Name = name;
        Raw = raw;
        _HashCode = GetHashCode(raw);
    }

    /// <summary>
    /// The scope of the resource.
    /// When null, the scope points to any scope.
    /// </summary>
    public string? Scope { get; }

    /// <summary>
    /// A unique name for the resource within the specified <see cref="Scope"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A string representation of the resource identifier.
    /// </summary>
    public string Raw { get; }

    /// <summary>
    /// The scope is not specified and can point to any scope.
    /// </summary>
    public bool AnyScope => Scope == null;

    /// <inheritdoc/>
    public override string ToString()
    {
        return Raw;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _HashCode;
    }

    /// <inheritdoc/>
    public bool Equals(ResourceIdReference other)
    {
        return Equals(other.Raw);
    }

    /// <inheritdoc/>
    public bool Equals(string other)
    {
        return string.Equals(Raw, other, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is ResourceIdReference id && Equals(id);
    }

    /// <inheritdoc/>
    public bool Equals(ResourceId other)
    {
        return ResourceIdEqualityComparer.IdEquals(other, this);
    }

    /// <summary>
    /// Compare two resource identifiers to determine if they are equal.
    /// </summary>
    public static bool operator ==(ResourceIdReference left, ResourceIdReference right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compare two resource identifiers to determine if they are not equal.
    /// </summary>
    public static bool operator !=(ResourceIdReference left, ResourceIdReference right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool TryParse(string? raw, out ResourceIdReference? value)
    {
        value = null;
        if (raw == null || !ResourceId.TryParseComponents(raw, out var scope, out var name) || name == null)
            return false;

        value = new ResourceIdReference(raw, scope, name);
        return true;
    }

    /// <summary>
    /// Create a <see cref="ResourceId"/> from the reference.
    /// </summary>
    /// <param name="kind">The kind of resource identifier.</param>
    /// <param name="defaultScope">The default scope to use if <see cref="Scope"/> is null.</param>
    /// <returns>A new <see cref="ResourceId"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When both <see cref="Scope"/> and <paramref name="defaultScope"/> are null.</exception>
    public ResourceId AsResourceId(ResourceIdKind kind, string? defaultScope = default)
    {
        if (Scope == null && defaultScope == null) throw new InvalidOperationException($"Cannot create ResourceId when both Scope and defaultScope are null.");

        return new ResourceId(Scope ?? defaultScope!, Name, kind);
    }

    private static int GetHashCode(string id)
    {
        return id.ToLowerInvariant().GetHashCode();
    }
}
