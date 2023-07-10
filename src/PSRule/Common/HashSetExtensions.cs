// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule
{
    internal static class HashSetExtensions
    {
        internal static bool ContainsIds(this HashSet<ResourceId> hashset, ResourceId id, ResourceId? @ref, ResourceId[] aliases, out ResourceId? duplicate)
        {
            duplicate = null;
            if (hashset == null || hashset.Count == 0)
                return false;

            if (hashset.Contains(id))
            {
                duplicate = id;
                return true;
            }
            if (@ref.HasValue && hashset.Contains(@ref.Value))
            {
                duplicate = @ref.Value;
                return true;
            }
            for (var i = 0; aliases != null && i < aliases.Length; i++)
            {
                if (hashset.Contains(aliases[i]))
                {
                    duplicate = aliases[i];
                    return true;
                }
            }
            return false;
        }

        internal static bool ContainsNames(this HashSet<string> hashset, ResourceId id, ResourceId? @ref, ResourceId[] aliases, out string duplicate)
        {
            duplicate = null;
            if (hashset == null || hashset.Count == 0)
                return false;

            if (hashset.Contains(id.Name))
            {
                duplicate = id.Name;
                return true;
            }
            if (@ref.HasValue && hashset.Contains(@ref.Value.Name))
            {
                duplicate = @ref.Value.Name;
                return true;
            }
            for (var i = 0; aliases != null && i < aliases.Length; i++)
            {
                if (hashset.Contains(aliases[i].Name))
                {
                    duplicate = aliases[i].Name;
                    return true;
                }
            }
            return false;
        }

        internal static void AddIds(this HashSet<ResourceId> hashset, ResourceId id, ResourceId? @ref, ResourceId[] aliases)
        {
            if (hashset == null)
                return;

            if (!hashset.Contains(id))
                hashset.Add(id);

            if (@ref.HasValue && !hashset.Contains(@ref.Value))
                hashset.Add(@ref.Value);

            for (var i = 0; aliases != null && i < aliases.Length; i++)
                if (!hashset.Contains(aliases[i]))
                    hashset.Add(aliases[i]);
        }

        internal static void AddNames(this HashSet<string> hashset, ResourceId id, ResourceId? @ref, ResourceId[] aliases)
        {
            if (hashset == null)
                return;

            if (!hashset.Contains(id.Name))
                hashset.Add(id.Name);

            if (@ref.HasValue && !hashset.Contains(@ref.Value.Name))
                hashset.Add(@ref.Value.Name);

            for (var i = 0; aliases != null && i < aliases.Length; i++)
                if (!hashset.Contains(aliases[i].Name))
                    hashset.Add(aliases[i].Name);
        }
    }
}
