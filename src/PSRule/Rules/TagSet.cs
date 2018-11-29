using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Rules
{
    public sealed class TagSet
    {
        private readonly Dictionary<string, string> _Tag;

        public TagSet()
        {
            _Tag = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private TagSet(Dictionary<string, string> tag)
        {
            _Tag = tag;
        }

        public int Count
        {
            get
            {
                return _Tag.Count;
            }
        }

        public bool Contains(object key, object value)
        {
            var k = key.ToString();
            var v = value.ToString();

            if (k == null || !_Tag.ContainsKey(k))
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(v, _Tag[k]);
        }

        public static TagSet FromHashtable(Hashtable hashtable)
        {
            if (hashtable == null || hashtable.Count == 0)
            {
                return null;
            }

            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry kv in hashtable)
            {
                dictionary[kv.Key.ToString()] = kv.Value.ToString();
            }

            return new TagSet(dictionary);
        }
    }
}
