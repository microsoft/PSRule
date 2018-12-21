using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Configuration
{
    public sealed class ExclusionOption : IDictionary<string, RuleExclusion>
    {
        private Dictionary<string, RuleExclusion> _Exclusions;

        public ExclusionOption()
        {
            _Exclusions = new Dictionary<string, RuleExclusion>(StringComparer.OrdinalIgnoreCase);
        }

        internal ExclusionOption(IDictionary<string, RuleExclusion> exclusions)
        {
            _Exclusions = new Dictionary<string, RuleExclusion>(exclusions, StringComparer.OrdinalIgnoreCase);
        }

        public RuleExclusion this[string key]
        {
            get => _Exclusions[key];
            set => _Exclusions[key] = value;
        }

        public ICollection<string> Keys => _Exclusions.Keys;

        public ICollection<RuleExclusion> Values => _Exclusions.Values;

        public int Count => _Exclusions.Count;

        public bool IsReadOnly => false;

        public void Add(string key, RuleExclusion value)
        {
            _Exclusions.Add(key, value);
        }

        public void Add(KeyValuePair<string, RuleExclusion> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Exclusions.Clear();
        }

        public bool Contains(KeyValuePair<string, RuleExclusion> item)
        {
            return ((IDictionary<string, RuleExclusion>)_Exclusions).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _Exclusions.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, RuleExclusion>[] array, int arrayIndex)
        {
            ((IDictionary<string, RuleExclusion>)_Exclusions).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, RuleExclusion>> GetEnumerator()
        {
            return _Exclusions.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _Exclusions.Remove(key);
        }

        public bool Remove(KeyValuePair<string, RuleExclusion> item)
        {
            return ((IDictionary<string, RuleExclusion>)_Exclusions).Remove(item);
        }

        public bool TryGetValue(string key, out RuleExclusion value)
        {
            return _Exclusions.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator ExclusionOption(Hashtable hashtable)
        {
            var option = new ExclusionOption();

            foreach (DictionaryEntry entry in hashtable)
            {
                var exclusion = RuleExclusion.FromObject(entry.Value);

                option._Exclusions.Add(entry.Key.ToString(), exclusion);
            }

            return option;
        }
    }
}