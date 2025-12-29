// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

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

    public static bool IdEquals(ResourceId x, ResourceIdReference y)
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

    private static bool EqualOrNull(string? x, string? y)
    {
        return x == null || y == null || StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }
}
