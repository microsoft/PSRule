// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using PSRule.Converters;
using PSRule.Data;
using PSRule.Definitions;

namespace PSRule;

/// <summary>
/// Extension methods for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Try to get a value and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopValue(this IDictionary<string, object> dictionary, string key, out object value)
    {
        return dictionary.TryGetValue(key, out value) && dictionary.Remove(key);
    }

    /// <summary>
    /// Try to get a value and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopValue<T>(this IDictionary<string, object> dictionary, string key, out T? value)
    {
        value = default;
        if (dictionary.TryGetValue(key, out var v) && dictionary.Remove(key) && v is T result)
        {
            value = result;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get a <see cref="bool"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopBool(this IDictionary<string, object> dictionary, string key, out bool value)
    {
        value = default;
        return TryPopValue(dictionary, key, out var v) && bool.TryParse(v.ToString(), out value);
    }

    /// <summary>
    /// Try to get a <typeparamref name="TEnum"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopEnum<TEnum>(this IDictionary<string, object> dictionary, string key, out TEnum value) where TEnum : struct
    {
        value = default;
        return TryPopValue(dictionary, key, out var v) && Enum.TryParse(v.ToString(), ignoreCase: true, result: out value);
    }

    /// <summary>
    /// Try to get a <see cref="string"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopString(this IDictionary<string, object> dictionary, string key, out string? value)
    {
        value = default;
        if (TryPopValue(dictionary, key, out var v) && v is string svalue)
        {
            value = svalue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get an array of strings and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopStringArray(this IDictionary<string, object> dictionary, string key, out string[]? value)
    {
        value = default;
        return TryPopValue(dictionary, key, out var v) && TypeConverter.TryStringOrArray(v, convert: true, value: out value);
    }

    /// <summary>
    /// Try to get a <see cref="StringArrayMap"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopStringArrayMap(this IDictionary<string, object> dictionary, string key, out StringArrayMap? value)
    {
        value = default;
        if (TryPopValue(dictionary, key, out var v) && v is StringArrayMap svalue)
        {
            value = svalue;
            return true;
        }
        if (v is Hashtable hashtable)
        {
            value = StringArrayMap.FromHashtable(hashtable);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get a <see cref="ResourceIdReference"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopResourceIdReference(this IDictionary<string, object> dictionary, string key, out ResourceIdReference? value)
    {
        value = default;

        if (TryPopValue(dictionary, key, out var v) && v is string s && ResourceIdReference.TryParse(s, out var reference))
        {
            value = reference;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Try to get an array of <see cref="ResourceIdReference"/> and remove it from the dictionary.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryPopResourceIdReferenceArray(this IDictionary<string, object> dictionary, string key, out ResourceIdReference[]? value)
    {
        value = default;

        if (TryPopStringArray(dictionary, key, out var strings) && strings != null)
        {
            var list = new List<ResourceIdReference>();
            foreach (var s in strings)
            {
                if (ResourceIdReference.TryParse(s, out var reference) && reference != null)
                {
                    list.Add(reference.Value);
                }
            }

            value = [.. list];
        }

        return value != default;
    }

    /// <summary>
    /// Try to get the value as a <see cref="bool"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetBool(this IDictionary<string, object> dictionary, string key, out bool? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (o is bool bvalue || (o is string svalue && bool.TryParse(svalue, out bvalue)))
        {
            value = bvalue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as a <see cref="long"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetLong(this IDictionary<string, object> dictionary, string key, out long? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (TypeConverter.TryLong(o, convert: true, value: out var i_value))
        {
            value = i_value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as a <see cref="int"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetInt(this IDictionary<string, object> dictionary, string key, out int? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (TypeConverter.TryInt(o, convert: true, value: out var i_value))
        {
            value = i_value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as a <see cref="char"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetChar(this IDictionary<string, object> dictionary, string key, out char? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (o is string svalue && svalue.Length == 1)
        {
            value = svalue[0];
            return true;
        }
        if (o is char cvalue)
        {
            value = cvalue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as a <see cref="string"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetString(this IDictionary<string, object> dictionary, string key, out string? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (o is string svalue)
        {
            value = svalue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as an <see cref="IEnumerable"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetEnumerable(this IDictionary<string, object> dictionary, string key, out IEnumerable? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (o is IEnumerable evalue)
        {
            value = evalue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Try to get the value as an array of strings.
    /// </summary>
    [DebuggerStepThrough]
    public static bool TryGetStringArray(this IDictionary<string, object> dictionary, string key, out string[]? value)
    {
        value = null;
        return dictionary.TryGetValue(key, out var o) && TypeConverter.TryStringOrArray(o, convert: true, value: out value);
    }

    /// <summary>
    /// Try to get the value as a dictionary.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetDictionary<T>(this IDictionary<string, object> dictionary, string key, out IDictionary<string, T>? value)
    {
        value = null;
        if (!dictionary.TryGetValue(key, out var o))
            return false;

        if (TypeConverter.TryDictionary<T>(o, out var d) && d != null)
        {
            value = d;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Add unique keys to the dictionary.
    /// Duplicate keys are ignored.
    /// </summary>
    [DebuggerStepThrough]
    public static void AddUnique<T>(this IDictionary<string, T> dictionary, IEnumerable<KeyValuePair<string, T>> values) where T : class
    {
        if (values == null) return;

        foreach (var kv in values)
        {
            if (!dictionary.ContainsKey(kv.Key))
                dictionary.Add(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// Add unique keys to the dictionary.
    /// Duplicate keys are ignored.
    /// </summary>
    [DebuggerStepThrough]
    public static void AddUnique(this IDictionary<string, string> dictionary, IEnumerable<KeyValuePair<string, string>> values)
    {
        if (values == null) return;

        foreach (var kv in values)
        {
            if (!dictionary.ContainsKey(kv.Key))
                dictionary.Add(kv.Key, kv.Value);
        }
    }

    internal static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new SortedDictionary<TKey, TValue>(dictionary);
    }

    internal static bool NullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return dictionary == null || dictionary.Count == 0;
    }
}
