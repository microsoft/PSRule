// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Converters;

namespace PSRule.Data;

/// <summary>
/// A mapping of string to string arrays.
/// </summary>
public sealed class StringArrayMap : IEnumerable<KeyValuePair<string, string[]>>
{
    private readonly Dictionary<string, string[]> _Map;

    /// <summary>
    /// Create an empty <see cref="StringArrayMap"/> instance.
    /// </summary>
    public StringArrayMap()
    {
        _Map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying an existing <see cref="StringArrayMap"/>.
    /// </summary>
    internal StringArrayMap(StringArrayMap map)
    {
        _Map = new Dictionary<string, string[]>(map._Map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped keys from a string dictionary.
    /// </summary>
    internal StringArrayMap(Dictionary<string, string[]> map)
    {
        _Map = new Dictionary<string, string[]>(map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped keys from a <seealso cref="Hashtable"/>.
    /// </summary>
    /// <param name="map"></param>
    internal StringArrayMap(Hashtable map)
        : this()
    {
        if (map != null) Load(this, map.IndexByString());
    }

    /// <summary>
    /// The number of mapped keys.
    /// </summary>
    public int Count => _Map.Count;

    /// <summary>
    /// Get or set mapping for a specified key.
    /// </summary>
    public string[] this[string key]
    {
        get
        {
            return !string.IsNullOrEmpty(key) && _Map.TryGetValue(key, out var value) ? value : Array.Empty<string>();
        }
        set
        {
            if (!string.IsNullOrEmpty(key))
                _Map[key] = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hashtable"></param>
    public static implicit operator StringArrayMap(Hashtable hashtable)
    {
        return new StringArrayMap(hashtable);
    }

    /// <summary>
    /// Try to get a mapping by key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">Returns an array of mapped keys.</param>
    /// <returns>Returns <c>true</c> if the key was found. Otherwise <c>false</c> is returned.</returns>
    public bool TryGetValue(string key, out string[] value)
    {
        return _Map.TryGetValue(key, out value);
    }

    /// <summary>
    /// Load a key map from an existing dictionary.
    /// </summary>
    internal static void Load(StringArrayMap map, IDictionary<string, object> properties)
    {
        foreach (var property in properties)
        {
            if (TypeConverter.TryStringOrArray(property.Value, convert: true, out var values) && values != null)
                map[property.Key] = values;
        }
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, string[]>>)_Map).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Convert the instance into a dictionary.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, string[]> ToDictionary()
    {
        return _Map;
    }

    /// <summary>
    /// Convert a hashtable into a <see cref="StringArrayMap"/> instance.
    /// </summary>
    public static StringArrayMap FromHashtable(Hashtable hashtable)
    {
        return new StringArrayMap(hashtable);
    }
}
