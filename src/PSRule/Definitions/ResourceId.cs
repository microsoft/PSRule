// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Runtime;

namespace PSRule.Definitions;

/// <summary>
/// Additional information about the type of identifier if available.
/// </summary>
internal enum ResourceIdKind
{
    /// <summary>
    /// Not specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// The identifier is a primary resource identifier.
    /// </summary>
    Id = 2,

    /// <summary>
    /// The identifier is a opaque reference resource identifier.
    /// </summary>
    Ref = 3,

    /// <summary>
    /// The identifier is an alias resource identifier.
    /// </summary>
    Alias = 4,
}

/// <summary>
/// A unique identifier for a resource.
/// </summary>
[DebuggerDisplay("{Value}")]
public struct ResourceId : IEquatable<ResourceId>, IEquatable<string>
{
    private const char SCOPE_SEPARATOR = '\\';

    private readonly int _HashCode;

    private ResourceId(string id, string scope, string name, ResourceIdKind kind)
    {
        Value = id;
        Scope = scope;
        Name = name;
        Kind = kind;
        _HashCode = GetHashCode(id);
    }

    internal ResourceId(string scope, string name, ResourceIdKind kind)
        : this(GetIdString(scope, name), LanguageScope.Normalize(scope), name, kind) { }

    /// <summary>
    /// A string representation of the resource identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The scope of the resource.
    /// </summary>
    public string Scope { get; }

    /// <summary>
    /// A unique name for the resource within the specified <see cref="Scope"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of resource identifier.
    /// </summary>
    internal ResourceIdKind Kind { get; }

    /// <summary>
    /// Converts the resource identifier to a string.
    /// </summary>
    /// <remarks>
    /// This is the same as <see cref="Value"/>.
    /// </remarks>
    /// <returns>A string representation of the resource identifier.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
        return _HashCode;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is ResourceId id && Equals(id);
    }

    /// <inheritdoc/>
    public bool Equals(ResourceId id)
    {
        return _HashCode == id._HashCode &&
            EqualOrNull(Scope, id.Scope) &&
            EqualOrNull(Name, id.Name);
    }

    /// <inheritdoc/>
    public bool Equals(string id)
    {
        return TryParse(id, out var scope, out var name) &&
            EqualOrNull(Scope, scope) &&
            EqualOrNull(Name, name);
    }

    /// <summary>
    /// Compare two resource identifiers to determine if they are equal.
    /// </summary>
    public static bool operator ==(ResourceId left, ResourceId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compare two resource identifiers to determine if they are not equal.
    /// </summary>
    public static bool operator !=(ResourceId left, ResourceId right)
    {
        return !left.Equals(right);
    }

    private static bool EqualOrNull(string x, string y)
    {
        return x == null || y == null || StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }

    private static int GetHashCode(string id)
    {
        return id.ToLowerInvariant().GetHashCode();
    }

    private static string GetIdString(string scope, string name)
    {
        return string.Concat(
            LanguageScope.Normalize(scope),
            SCOPE_SEPARATOR,
            name
        );
    }

    internal static ResourceId Parse(string id, ResourceIdKind kind = ResourceIdKind.Unknown)
    {
        return TryParse(id, kind, out var value) ? value.Value : default;
    }

    private static bool TryParse(string id, ResourceIdKind kind, out ResourceId? value)
    {
        value = null;
        if (string.IsNullOrEmpty(id) || !TryParse(id, out var scope, out var name))
            return false;

        value = new ResourceId(id, scope, name, kind);
        return true;
    }

    private static bool TryParse(string id, out string scope, out string name)
    {
        scope = null;
        name = null;
        if (string.IsNullOrEmpty(id))
            return false;

        var scopeSeparatorIndex = id.IndexOf(SCOPE_SEPARATOR);
        scope = scopeSeparatorIndex >= 0 ? id.Substring(0, scopeSeparatorIndex) : null;
        name = id.Substring(scopeSeparatorIndex + 1);
        return true;
    }
}

/// <summary>
/// Compares two resource identifiers to determine if they are equal.
/// </summary>
internal sealed class ResourceIdEqualityComparer : IEqualityComparer<ResourceId>, IEqualityComparer<string>
{
    public static readonly ResourceIdEqualityComparer Default = new();

    public static bool IdEquals(ResourceId x, ResourceId y)
    {
        return EqualOrNull(x.Scope, y.Scope) &&
            EqualOrNull(x.Name, y.Name);
    }

    public static bool IdEquals(ResourceId x, string y)
    {
        return x.Equals(y);
    }

    public static bool IdEquals(string x, string y)
    {
        ResourceHelper.ParseIdString(x, out var scope_x, out var name_x);
        ResourceHelper.ParseIdString(y, out var scope_y, out var name_y);
        return EqualOrNull(scope_x, scope_y) &&
            EqualOrNull(name_x, name_y);
    }

    #region IEqualityComparer<ResourceId>

    public bool Equals(ResourceId x, ResourceId y)
    {
        return IdEquals(x, y);
    }

    public int GetHashCode(ResourceId obj)
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (obj.Scope != null ? obj.Scope.GetHashCode() : 0);
            hash = hash * 23 + (obj.Name != null ? obj.Name.GetHashCode() : 0);
            return hash;
        }
    }

    #endregion IEqualityComparer<ResourceId>

    #region IEqualityComparer<string>

    public bool Equals(string x, string y)
    {
        return IdEquals(x, y);
    }

    public int GetHashCode(string obj)
    {
        ResourceHelper.ParseIdString(obj, out var scope, out var name);
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (scope != null ? scope.GetHashCode() : 0);
            hash = hash * 23 + (name != null ? name.GetHashCode() : 0);
            return hash;
        }
    }

    #endregion IEqualityComparer<string>

    private static bool EqualOrNull(string x, string y)
    {
        return x == null || y == null || StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }
}
