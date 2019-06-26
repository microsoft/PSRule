﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace PSRule.Rules
{
    public sealed class TagSet : DynamicObject
    {
        private readonly IEqualityComparer<string> _ValueComparer;
        private readonly Dictionary<string, string> _Tag;

        public TagSet()
        {
            _ValueComparer = StringComparer.OrdinalIgnoreCase;
            _Tag = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private TagSet(Dictionary<string, string> tag)
        {
            _ValueComparer = StringComparer.OrdinalIgnoreCase;
            _Tag = tag;
        }

        public int Count => _Tag.Count;

        public string this[string key] => _Tag[key];

        public bool Contains(object key, object value)
        {
            var k = key.ToString();
            var v = value.ToString();

            if (k == null || !_Tag.ContainsKey(k))
            {
                return false;
            }
            else if (v == "*")
            {
                return true;
            }

            return _ValueComparer.Equals(v, _Tag[k]);
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

        internal static TagSet FromDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            return new TagSet(dictionary);
        }

        public Hashtable ToHashtable()
        {
            return new Hashtable(_Tag, StringComparer.OrdinalIgnoreCase);
        }

        public string ToViewString()
        {
            var sb = new StringBuilder();
            var i = 0;

            foreach (var kv in _Tag)
            {
                if (i > 0)
                {
                    sb.Append(Environment.NewLine);
                }

                sb.Append(kv.Key.ToString());
                sb.Append('=');
                sb.Append('\'');
                sb.Append(kv.Value.ToString());
                sb.Append('\'');
                i++;
            }

            return sb.ToString();
        }

        public bool ContainsKey(string key)
        {
            return _Tag.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _Tag.TryGetValue(key, out value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var found = _Tag.TryGetValue(binder.Name, out string value);
            result = value;
            return found;
        }
    }
}
