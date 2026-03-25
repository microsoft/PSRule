// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Dynamic;
using Newtonsoft.Json;

namespace PSRule.Configuration;

/// <summary>
/// A mapping of fields to property names.
/// </summary>
[JsonConverter(typeof(FieldMapJsonConverter))]
public sealed class FieldMap : DynamicObject, IEnumerable<KeyValuePair<string, string[]>>
{
    private readonly Dictionary<string, string[]> _Map;

    /// <summary>
    /// Create an empty <see cref="FieldMap"/> instance.
    /// </summary>
    public FieldMap()
    {
        _Map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying an existing <see cref="FieldMap"/>.
    /// </summary>
    internal FieldMap(FieldMap map)
    {
        _Map = new Dictionary<string, string[]>(map._Map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped fields from a string dictionary.
    /// </summary>
    internal FieldMap(Dictionary<string, string[]> map)
    {
        _Map = new Dictionary<string, string[]>(map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped fields from a <seealso cref="Hashtable"/>.
    /// </summary>
    /// <param name="map"></param>
    internal FieldMap(Hashtable map)
        : this()
    {
        var index = PSRuleOption.BuildIndex(map);
        Load(this, index);
    }

    /// <summary>
    /// The number of mapped fields.
    /// </summary>
    public int Count => _Map.Count;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hashtable"></param>
    public static implicit operator FieldMap(Hashtable hashtable)
    {
        return new FieldMap(hashtable);
    }

    /// <summary>
    /// Try to get a field mapping by name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fields">Returns an array of mapped fields when the field name was found.</param>
    /// <returns>Returns <c>true</c> if the field name was found. Otherwise <c>false</c> is returned.</returns>
    public bool TryField(string fieldName, out string[] fields)
    {
        return _Map.TryGetValue(fieldName, out fields);
    }

    /// <summary>
    /// Set a field mapping.
    /// </summary>
    internal void Set(string fieldName, string[] fields)
    {
        _Map[fieldName] = fields;
    }

    /// <summary>
    /// Load a field map from an existing dictionary.
    /// </summary>
    internal static void Load(FieldMap map, Dictionary<string, object> properties)
    {
        foreach (var property in properties)
        {
            if (property.Value is string value && !string.IsNullOrEmpty(value))
                map.Set(property.Key, new string[] { value });
            else if (property.Value is string[] array && array.Length > 0)
                map.Set(property.Key, array);
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

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var found = TryField(binder.Name, out var value);
        result = value;
        return found;
    }

    internal IDictionary<string, string[]> GetFieldMap
    {
        get => _Map;
    }

    internal static FieldMap Combine(FieldMap? m1, FieldMap? m2)
    {
        if (m1 == null) return m2 ?? new FieldMap();
        if (m2 == null) return m1;

        var result = new FieldMap(m1);
        result._Map.AddUnique(m2._Map);
        return result;
    }
}
