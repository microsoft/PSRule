// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Definitions;

/// <summary>
/// A string index map for child values.
/// </summary>
/// <typeparam name="TValue">The type of child values assigned for each map key.</typeparam>
public abstract class StringMap<TValue> : IStringMap<TValue>, IEnumerable<KeyValuePair<string, TValue>>, IYamlConvertible where TValue : class
{
    private readonly Dictionary<string, TValue> _Items;

    /// <summary>
    /// Create an empty map.
    /// </summary>
    protected internal StringMap()
    {
        _Items = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a map with items.
    /// </summary>
    protected internal StringMap(IDictionary<string, TValue> items)
    {
        _Items = new Dictionary<string, TValue>(items, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a map based on an existing map.
    /// </summary>
    protected internal StringMap(StringMap<TValue> map)
        : this()
    {
        Combine(map);
    }

    /// <inheritdoc/>
    public int Count => _Items.Count;

    /// <inheritdoc/>
    public TValue? this[string key]
    {
        get
        {
            return _Items.TryGetValue(key, out var value) ? value : default;
        }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _Items[key] = value;
        }
    }

    /// <inheritdoc/>
    public void Add(string key, TValue value)
    {
        _Items.Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, TValue> item)
    {
        _Items.Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, out TValue value)
    {
        return _Items.TryGetValue(key, out value);
    }

    /// <summary>
    /// Clear the map of all keys and values.
    /// </summary>
    public void Clear()
    {
        _Items.Clear();
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _Items.GetHashCode();
    }

    /// <summary>
    /// Add unique keys from another map.
    /// </summary>
    protected void Combine(StringMap<TValue> map)
    {
        if (map == null || map._Items.Count == 0)
            return;

        _Items.AddUnique(map._Items);
    }

    /// <summary>
    /// Load values from a key/ value dictionary into the option.
    /// Keys that appear in both will replaced by dictionary values.
    /// </summary>
    /// <exception cref="ArgumentNullException">Is raised if the dictionary is null.</exception>
    protected void ImportFromDictionary(string prefix, IDictionary<string, object> dictionary, Func<KeyValuePair<string, object>, KeyValuePair<string, TValue>?> format)
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (format == null) throw new ArgumentNullException(nameof(format));

        if (dictionary.Count == 0)
            return;

        foreach (var variable in dictionary)
        {
            if (TryKeyPrefix(variable.Key, prefix, out _))
            {
                var formatted = format(variable);
                if (formatted != null)
                {
                    _Items[formatted.Value.Key] = formatted.Value.Value;
                }
            }
        }
    }

    /// <summary>
    /// Import values from environment variables.
    /// </summary>
    protected void ImportFromEnvironmentVariables(string prefix, Func<KeyValuePair<string, object>, KeyValuePair<string, TValue>?> format)
    {
        if (format == null) throw new ArgumentNullException(nameof(format));

        foreach (var variable in Environment.GetByPrefix(prefix))
        {
            if (TryKeyPrefix(variable.Key, prefix, out _))
            {
                var formatted = format(variable);
                if (formatted != null)
                {
                    _Items[formatted.Value.Key] = formatted.Value.Value;
                }
            }
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

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
    {
        return _Items.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _Items.GetEnumerator();
    }

    /// <summary>
    /// Deserialize the string map.
    /// </summary>
    void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
    {
        if (!parser.TryConsume<MappingStart>(out _))
            return;

        while (parser.TryConsume<Scalar>(out var scalar))
        {
            var key = scalar.Value;
            if (!string.IsNullOrEmpty(key) && nestedObjectDeserializer.Invoke(typeof(TValue)) is TValue value)
                _Items.Add(key, value);
        }

        parser.Require<MappingEnd>();
        parser.MoveNext();
    }

    /// <summary>
    /// Serialize the string map.
    /// </summary>
    void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        emitter.Emit(new MappingStart());

        foreach (var kv in _Items)
        {
            emitter.Emit(new Scalar(kv.Key));
            nestedObjectSerializer.Invoke(kv.Value, typeof(TValue));
        }

        emitter.Emit(new MappingEnd());
    }
}
