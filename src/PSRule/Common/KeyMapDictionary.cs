// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace PSRule
{
    public abstract class KeyMapDictionary<TValue> : DynamicObject, IDictionary<string, TValue>
    {
        private readonly Dictionary<string, TValue> _Map;

        protected KeyMapDictionary()
        {
            _Map = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        }

        protected KeyMapDictionary(KeyMapDictionary<TValue> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            _Map = new Dictionary<string, TValue>(map._Map, StringComparer.OrdinalIgnoreCase);
        }

        protected KeyMapDictionary(IDictionary<string, TValue> dictionary)
        {
            _Map = dictionary == null ?
                new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase) :
                new Dictionary<string, TValue>(dictionary, StringComparer.OrdinalIgnoreCase);
        }

        protected KeyMapDictionary(Hashtable hashtable)
            : this()
        {
            Load(hashtable);
        }

        public TValue this[string key]
        {
            get => _Map[key];
            set => _Map[key] = value;
        }

        public ICollection<string> Keys => _Map.Keys;

        public ICollection<TValue> Values => _Map.Values;

        public int Count => _Map.Count;

        public bool IsReadOnly => false;

        public void Add(string key, TValue value)
        {
            _Map.Add(key, value);
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Map.Clear();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return ((IDictionary<string, TValue>)_Map).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _Map.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<string, TValue>)_Map).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _Map.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _Map.Remove(key);
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return ((IDictionary<string, TValue>)_Map).Remove(item);
        }

        public bool TryGetValue(string key, out TValue value)
        {
            return _Map.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Load options from a hashtable.
        /// </summary>
        protected void Load(Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            foreach (DictionaryEntry entry in hashtable)
                _Map.Add(entry.Key.ToString(), (TValue)entry.Value);
        }

        /// <summary>
        /// Load options from environment variables.
        /// </summary>
        internal void Load(string prefix, EnvironmentHelper env, Func<string, string> format = null)
        {
            if (env == null)
                throw new ArgumentNullException(nameof(env));

            foreach (var variable in env.WithPrefix(prefix))
            {
                if (TryKeyPrefix(variable.Key, prefix, out string suffix))
                {
                    if (format != null)
                        suffix = format(suffix);

                    _Map[suffix] = (TValue)variable.Value;
                }
            }
        }

        /// <summary>
        /// Load options from a dictionary.
        /// </summary>
        protected void Load(string prefix, IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.Count == 0)
                return;

            var keys = dictionary.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                if (TryKeyPrefix(keys[i], prefix, out string suffix) && dictionary.TryPopValue(keys[i], out object value))
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

        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            var found = _Map.TryGetValue(binder.Name, out TValue value);
            result = value;
            return found;
        }
    }
}
