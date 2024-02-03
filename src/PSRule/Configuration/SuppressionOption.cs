// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Configuration;

/// <summary>
/// Options that affect rule suppression during execution.
/// </summary>
public sealed class SuppressionOption : IDictionary<string, SuppressionRule>
{
    private readonly Dictionary<string, SuppressionRule> _Rules;

    /// <summary>
    /// Creates an empty suppression option.
    /// </summary>
    public SuppressionOption()
    {
        _Rules = new Dictionary<string, SuppressionRule>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a suppression option by loading <see cref="SuppressionRule"/> from a dictionary.
    /// </summary>
    /// <param name="rules">A dictionary of <see cref="SuppressionRule"/>.</param>
    internal SuppressionOption(IDictionary<string, SuppressionRule> rules)
    {
        _Rules = rules == null ?
           new Dictionary<string, SuppressionRule>(StringComparer.OrdinalIgnoreCase) :
           new Dictionary<string, SuppressionRule>(rules, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get a <see cref="SuppressionRule"/> indexed by rule name.
    /// </summary>
    /// <param name="key">The name of the rule.</param>
    /// <returns>A matching <see cref="SuppressionRule"/>.</returns>
    public SuppressionRule this[string key]
    {
        get => _Rules[key];
        set => _Rules[key] = value;
    }

    /// <inheritdoc/>
    public ICollection<string> Keys => _Rules.Keys;

    /// <inheritdoc/>
    public ICollection<SuppressionRule> Values => _Rules.Values;

    /// <inheritdoc/>
    public int Count => _Rules.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <summary>
    /// Add a suppression rule to the option by rule name.
    /// </summary>
    /// <param name="key">The name of the rule to apply the suppression rule to.</param>
    /// <param name="value">A <see cref="SuppressionRule"/> to map to the rule.</param>
    public void Add(string key, SuppressionRule value)
    {
        _Rules.Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, SuppressionRule> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clear all suppression rules.
    /// </summary>
    public void Clear()
    {
        _Rules.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, SuppressionRule> item)
    {
        return ((IDictionary<string, SuppressionRule>)_Rules).Contains(item);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return _Rules.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, SuppressionRule>[] array, int arrayIndex)
    {
        ((IDictionary<string, SuppressionRule>)_Rules).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, SuppressionRule>> GetEnumerator()
    {
        return _Rules.GetEnumerator();
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static SuppressionOption Combine(SuppressionOption o1, SuppressionOption o2)
    {
        var result = new SuppressionOption(o1);
        result.AddUnique(o2);
        return result;
    }

    /// <summary>
    /// Remove a specific <see cref="SuppressionRule"/> by rule name.
    /// </summary>
    /// <param name="key">The name of the rule.</param>
    /// <returns>Returns <c>true</c> if the element was found and removed.</returns>
    public bool Remove(string key)
    {
        return _Rules.Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, SuppressionRule> item)
    {
        return ((IDictionary<string, SuppressionRule>)_Rules).Remove(item);
    }

    /// <summary>
    /// Try to get a <see cref="SuppressionRule"/> from the specified rule name.
    /// </summary>
    /// <param name="key">The name of the rule.</param>
    /// <param name="value">A <see cref="SuppressionRule"/> if any match the specified rule name.</param>
    /// <returns>Returns <c>true</c> if the key was found and <paramref name="value"/> returned.</returns>
    public bool TryGetValue(string key, out SuppressionRule value)
    {
        return _Rules.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Convert a hashtable to suppression options.
    /// </summary>
    /// <param name="hashtable">A hashtable of <see cref="SuppressionRule"/> indexed by rule name.</param>
    public static implicit operator SuppressionOption(Hashtable hashtable)
    {
        var option = new SuppressionOption();
        foreach (DictionaryEntry entry in hashtable)
        {
            var rule = SuppressionRule.FromObject(entry.Value);
            option._Rules.Add(entry.Key.ToString(), rule);
        }
        return option;
    }
}
