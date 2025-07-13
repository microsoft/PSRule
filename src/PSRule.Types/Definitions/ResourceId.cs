// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions;

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
        : this(GetIdString(scope, name), ResourceHelper.NormalizeScope(scope), name, kind) { }

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

    private static bool EqualOrNull(string? x, string? y)
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
            ResourceHelper.NormalizeScope(scope),
            SCOPE_SEPARATOR,
            name
        );
    }

    internal static ResourceId Parse(string id, ResourceIdKind kind = ResourceIdKind.Unknown)
    {
        return TryParse(id, kind, out var value) && value != null ? value.Value : default;
    }

    private static bool TryParse(string id, ResourceIdKind kind, out ResourceId? value)
    {
        value = null;
        if (string.IsNullOrEmpty(id) || !TryParse(id, out var scope, out var name) || name == null)
            return false;

        scope ??= ResourceHelper.STANDALONE_SCOPE_NAME;
        value = new ResourceId(id, scope, name, kind);
        return true;
    }

    private static bool TryParse(string id, out string? scope, out string? name)
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
