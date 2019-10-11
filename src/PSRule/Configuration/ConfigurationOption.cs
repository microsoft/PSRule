// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace PSRule.Configuration
{
    /// <summary>
    /// A set of configuration values that can be used within rule definitions.
    /// </summary>
    public sealed class ConfigurationOption : DynamicObject, IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _Configuration;

        public ConfigurationOption()
        {
            _Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        internal ConfigurationOption(IDictionary<string, object> rules)
        {
            _Configuration = new Dictionary<string, object>(rules, StringComparer.OrdinalIgnoreCase);
        }

        public object this[string key]
        {
            get => _Configuration[key];
            set => _Configuration[key] = value;
        }

        public ICollection<string> Keys => _Configuration.Keys;

        public ICollection<object> Values => _Configuration.Values;

        public int Count => _Configuration.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _Configuration.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Configuration.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_Configuration).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _Configuration.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_Configuration).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _Configuration.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _Configuration.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_Configuration).Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _Configuration.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator ConfigurationOption(Hashtable hashtable)
        {
            var option = new ConfigurationOption();

            foreach (DictionaryEntry entry in hashtable)
            {
                option._Configuration.Add(entry.Key.ToString(), entry.Value);
            }

            return option;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var found = _Configuration.TryGetValue(binder.Name, out object value);
            result = value;
            return found;
        }
    }
}
