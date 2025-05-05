// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule;

internal static class HashSetExtensions
{
    internal static bool ContainsIds(this HashSet<ResourceId> set, ResourceId id, ResourceId? @ref, ResourceId[]? aliases, out ResourceId? duplicate)
    {
        duplicate = null;
        if (set == null || set.Count == 0)
            return false;

        if (set.Contains(id))
        {
            duplicate = id;
            return true;
        }
        if (@ref.HasValue && set.Contains(@ref.Value))
        {
            duplicate = @ref.Value;
            return true;
        }
        for (var i = 0; aliases != null && i < aliases.Length; i++)
        {
            if (set.Contains(aliases[i]))
            {
                duplicate = aliases[i];
                return true;
            }
        }
        return false;
    }

    internal static bool ContainsNames(this HashSet<string> set, ResourceId id, ResourceId? @ref, ResourceId[]? aliases, out string? duplicate)
    {
        duplicate = null;
        if (set == null || set.Count == 0)
            return false;

        if (set.Contains(id.Name))
        {
            duplicate = id.Name;
            return true;
        }
        if (@ref.HasValue && set.Contains(@ref.Value.Name))
        {
            duplicate = @ref.Value.Name;
            return true;
        }
        for (var i = 0; aliases != null && i < aliases.Length; i++)
        {
            if (set.Contains(aliases[i].Name))
            {
                duplicate = aliases[i].Name;
                return true;
            }
        }
        return false;
    }

    internal static void AddIds(this HashSet<ResourceId> set, ResourceId id, ResourceId? @ref, ResourceId[]? aliases)
    {
        if (set == null)
            return;

        if (!set.Contains(id))
            set.Add(id);

        if (@ref.HasValue && !set.Contains(@ref.Value))
            set.Add(@ref.Value);

        for (var i = 0; aliases != null && i < aliases.Length; i++)
            if (!set.Contains(aliases[i]))
                set.Add(aliases[i]);
    }

    internal static void AddNames(this HashSet<string> set, ResourceId id, ResourceId? @ref, ResourceId[]? aliases)
    {
        if (set == null)
            return;

        if (!set.Contains(id.Name))
            set.Add(id.Name);

        if (@ref.HasValue && !set.Contains(@ref.Value.Name))
            set.Add(@ref.Value.Name);

        for (var i = 0; aliases != null && i < aliases.Length; i++)
            if (!set.Contains(aliases[i].Name))
                set.Add(aliases[i].Name);
    }
}
