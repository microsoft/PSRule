// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Configuration
{
    public sealed class SuppressionOption : IDictionary<string, SuppressionRule>
    {
        private readonly Dictionary<string, SuppressionRule> _Rules;

        public SuppressionOption()
        {
            _Rules = new Dictionary<string, SuppressionRule>(StringComparer.OrdinalIgnoreCase);
        }

        internal SuppressionOption(IDictionary<string, SuppressionRule> rules)
        {
            _Rules = new Dictionary<string, SuppressionRule>(rules, StringComparer.OrdinalIgnoreCase);
        }

        public SuppressionRule this[string key]
        {
            get => _Rules[key];
            set => _Rules[key] = value;
        }

        public ICollection<string> Keys => _Rules.Keys;

        public ICollection<SuppressionRule> Values => _Rules.Values;

        public int Count => _Rules.Count;

        public bool IsReadOnly => false;

        public void Add(string key, SuppressionRule value)
        {
            _Rules.Add(key, value);
        }

        public void Add(KeyValuePair<string, SuppressionRule> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Rules.Clear();
        }

        public bool Contains(KeyValuePair<string, SuppressionRule> item)
        {
            return ((IDictionary<string, SuppressionRule>)_Rules).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _Rules.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, SuppressionRule>[] array, int arrayIndex)
        {
            ((IDictionary<string, SuppressionRule>)_Rules).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, SuppressionRule>> GetEnumerator()
        {
            return _Rules.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _Rules.Remove(key);
        }

        public bool Remove(KeyValuePair<string, SuppressionRule> item)
        {
            return ((IDictionary<string, SuppressionRule>)_Rules).Remove(item);
        }

        public bool TryGetValue(string key, out SuppressionRule value)
        {
            return _Rules.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
}