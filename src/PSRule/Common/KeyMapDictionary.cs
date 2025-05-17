// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Dynamic;

namespace PSRule;

/// <summary>
/// A dictionary of key/ value pairs indexed by a string key that is case-insensitive.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class KeyMapDictionary<TValue> : DynamicObject, IDictionary<string, TValue>
{
    private readonly Dictionary<string, TValue> _Map;

    /// <summary>
    /// Create an empty map.
    /// </summary>
    protected internal KeyMapDictionary()
    {
        _Map = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a map initially populated with values copied from an existing instance.
    /// </summary>
    /// <param name="map">An existing instance to copy key/ values from.</param>
    /// <exception cref="ArgumentNullException">Is raised if the map is null.</exception>
    protected internal KeyMapDictionary(KeyMapDictionary<TValue> map)
    {
        _Map = map == null ?
            new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase) :
            new Dictionary<string, TValue>(map._Map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a map initially populated with values copied from a dictionary.
    /// </summary>
    /// <param name="dictionary">An existing dictionary to copy key/ values from.</param>
    protected internal KeyMapDictionary(IDictionary<string, TValue> dictionary)
    {
        _Map = dictionary == null ?
            new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase) :
            new Dictionary<string, TValue>(dictionary, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a map initially populated with values copied from a hashtable.
    /// </summary>
    /// <param name="hashtable">An existing hashtable to copy key/ values from.</param>
    protected internal KeyMapDictionary(Hashtable hashtable)
        : this()
    {
        Load(hashtable);
    }

    /// <inheritdoc/>
    public TValue this[string key]
    {
        get => _Map[key];
        set => _Map[key] = value;
    }

    /// <inheritdoc/>
    public ICollection<string> Keys => _Map.Keys;

    /// <inheritdoc/>
    public ICollection<TValue> Values => _Map.Values;

    /// <inheritdoc/>
    public int Count => _Map.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(string key, TValue value)
    {
        _Map.Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clear the map of all keys and values.
    /// </summary>
    public void Clear()
    {
        _Map.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, TValue> item)
    {
        return ((IDictionary<string, TValue>)_Map).Contains(item);
    }

    /// <summary>
    /// Determines if a specified key exists in the map.
    /// </summary>
    /// <param name="key">The key map.</param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return _Map.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
        ((IDictionary<string, TValue>)_Map).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
    {
        return _Map.GetEnumerator();
    }

    /// <summary>
    /// Remove the key/ value from the map by key.
    /// </summary>
    /// <param name="key">The key of the key/ value to remove.</param>
    /// <returns>Returns <c>true</c> if the element was found and removed.</returns>
    public bool Remove(string key)
    {
        return _Map.Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, TValue> item)
    {
        return ((IDictionary<string, TValue>)_Map).Remove(item);
    }

    /// <summary>
    /// Try to get the value from the specified key.
    /// </summary>
    /// <param name="key">The specific key to find in the map.</param>
    /// <param name="value">The value of the specific key.</param>
    /// <returns>Returns <c>true</c> if the key was found and <paramref name="value"/> returned.</returns>
    public bool TryGetValue(string key, out TValue value)
    {
        return _Map.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Load options from a hashtable.
    /// </summary>
    /// <exception cref="ArgumentNullException">Is raised if the hashtable is null.</exception>
    protected void Load(Hashtable hashtable)
    {
        if (hashtable == null)
            throw new ArgumentNullException(nameof(hashtable));

        foreach (DictionaryEntry entry in hashtable)
            _Map.Add(entry.Key.ToString(), (TValue)entry.Value);
    }

    /// <summary>
    /// Load values from environment variables into the option.
    /// Keys that appear in both will replaced by environment variable values.
    /// </summary>
    /// <exception cref="ArgumentNullException">Is raised if the environment helper is null.</exception>
    internal void Load(string prefix, Func<string, string>? format = null)
    {
        foreach (var variable in Environment.GetByPrefix(prefix))
        {
            if (TryKeyPrefix(variable.Key, prefix, out var suffix))
            {
                if (format != null)
                    suffix = format(suffix);

                _Map[suffix] = (TValue)variable.Value;
            }
        }
    }

    /// <summary>
    /// Load values from a key/ value dictionary into the option.
    /// Keys that appear in both will replaced by dictionary values.
    /// </summary>
    /// <exception cref="ArgumentNullException">Is raised if the dictionary is null.</exception>
    protected void Load(string prefix, IDictionary<string, object> dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (dictionary.Count == 0)
            return;

        var keys = dictionary.Keys.ToArray();
        for (var i = 0; i < keys.Length; i++)
        {
            if (TryKeyPrefix(keys[i], prefix, out var suffix) && dictionary.TryPopValue(keys[i], out var value))
                _Map[suffix] = (TValue)value;
        }
    }

    /// <summary>
    /// Try a key prefix.
    /// </summary>
    private static bool TryKeyPrefix(string key, string prefix, out string suffix)
    {
        suffix = key;
        if (prefix == null || prefix.Length == 0)
            return true;

        if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            suffix = key.Substring(prefix.Length);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get the value of a dynamic object member.
    /// </summary>
    /// <param name="binder">A dynamic binder object.</param>
    /// <param name="result">The value of the member.</param>
    /// <returns>Returns <c>true</c> if the member was found and <c>false</c> if the member was not.</returns>
    /// <exception cref="ArgumentNullException">Is raised if the binder is null.</exception>
    public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (binder == null)
            throw new ArgumentNullException(nameof(binder));

        var found = _Map.TryGetValue(binder.Name, out var value);
        result = value;
        return found;
    }
}
