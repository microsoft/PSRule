// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Definitions;

/// <summary>
/// Additional resource taxonomy references.
/// </summary>
public sealed class ResourceLabels : Dictionary<string, string[]>, IResourceLabels
{
    /// <summary>
    /// Create an empty set of resource labels.
    /// </summary>
    public ResourceLabels() : base(StringComparer.OrdinalIgnoreCase) { }

    /// <summary>
    /// Convert from a hashtable to resource labels.
    /// </summary>
    internal static ResourceLabels? FromHashtable(Hashtable hashtable)
    {
        if (hashtable == null || hashtable.Count == 0)
            return null;

        var annotations = new ResourceLabels();
        foreach (DictionaryEntry kv in hashtable)
        {
            var key = kv.Key.ToString();
            if (hashtable.TryGetStringArray(key, out var value))
                annotations[key] = value;
        }
        return annotations;
    }

    /// <inheritdoc/>
    public bool Contains(string key, string[] value)
    {
        if (!TryGetValue(key, out var actual))
            return false;

        if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == "*"))
            return true;

        for (var i = 0; i < value.Length; i++)
        {
            if (Array.IndexOf(actual, value[i]) != -1)
                return true;
        }
        return false;
    }
}
