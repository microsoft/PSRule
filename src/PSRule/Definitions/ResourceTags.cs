// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Text;

namespace PSRule.Definitions;

/// <summary>
/// Additional resource tags.
/// </summary>
public sealed class ResourceTags : Dictionary<string, string>
{
    private Hashtable _Hashtable;

    /// <summary>
    /// Create an empty set of resource tags.
    /// </summary>
    [DebuggerStepThrough]
    public ResourceTags() : base(StringComparer.OrdinalIgnoreCase) { }

    /// <summary>
    /// Convert from a hashtable to resource tags.
    /// </summary>
    [DebuggerStepThrough]
    internal static ResourceTags FromHashtable(Hashtable hashtable)
    {
        if (hashtable == null || hashtable.Count == 0)
            return null;

        var tags = new ResourceTags();
        foreach (DictionaryEntry kv in hashtable)
            tags[kv.Key.ToString()] = kv.Value.ToString();

        return tags;
    }

    /// <summary>
    /// Convert from a dictionary of string pairs to resource tags.
    /// </summary>
    [DebuggerStepThrough]
    internal static ResourceTags FromDictionary(Dictionary<string, string> dictionary)
    {
        if (dictionary == null)
            return null;

        var tags = new ResourceTags();
        foreach (var kv in dictionary)
            tags[kv.Key] = kv.Value;

        return tags;
    }

    /// <summary>
    /// Convert resource tags to a hashtable.
    /// </summary>
    [DebuggerStepThrough]
    public Hashtable ToHashtable()
    {
        _Hashtable ??= new ReadOnlyHashtable(this, StringComparer.OrdinalIgnoreCase);
        return _Hashtable;
    }

    /// <summary>
    /// Check if a specific resource tag exists.
    /// </summary>
    internal bool Contains(object key, object value)
    {
        if (key == null || value == null || key is not string k || !ContainsKey(k))
            return false;

        if (TryArray(value, out var values))
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (Comparer.Equals(values[i], this[k]))
                    return true;
            }
            return false;
        }
        var v = value.ToString();
        return v == "*" || Comparer.Equals(v, this[k]);
    }

    private static bool TryArray(object o, out string[] values)
    {
        values = null;
        if (o is string[] sArray)
        {
            values = sArray;
            return true;
        }
        if (o is IEnumerable<object> oValues)
        {
            var result = new List<string>();
            foreach (var obj in oValues)
                result.Add(obj.ToString());

            values = result.ToArray();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Convert the resourecs tags to a display string for PowerShell views.
    /// </summary>
    /// <returns></returns>
    public string ToViewString()
    {
        var sb = new StringBuilder();
        var i = 0;

        foreach (var kv in this)
        {
            if (i > 0)
                sb.Append(System.Environment.NewLine);

            sb.Append(kv.Key);
            sb.Append('=');
            sb.Append('\'');
            sb.Append(kv.Value);
            sb.Append('\'');
            i++;
        }
        return sb.ToString();
    }
}
