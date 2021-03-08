// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace PSRule.Configuration
{
    /// <summary>
    /// A mapping of fields to property names.
    /// </summary>
    public sealed class FieldMap : DynamicObject, IEnumerable<KeyValuePair<string, string[]>>
    {
        private readonly Dictionary<string, string[]> _Map;

        public FieldMap()
        {
            _Map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        internal FieldMap(FieldMap map)
        {
            _Map = new Dictionary<string, string[]>(map._Map, StringComparer.OrdinalIgnoreCase);
        }

        internal FieldMap(Dictionary<string, string[]> map)
        {
            _Map = new Dictionary<string, string[]>(map, StringComparer.OrdinalIgnoreCase);
        }

        internal FieldMap(Hashtable map)
            : this()
        {
            var index = PSRuleOption.BuildIndex(map);
            Load(this, index);
        }

        public int Count => _Map.Count;

        public static implicit operator FieldMap(Hashtable hashtable)
        {
            return new FieldMap(hashtable);
        }

        public bool TryField(string fieldName, out string[] fields)
        {
            return _Map.TryGetValue(fieldName, out fields);
        }

        internal void Set(string fieldName, string[] fields)
        {
            _Map[fieldName] = fields;
        }

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

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string[]>>)_Map).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var found = TryField(binder.Name, out string[] value);
            result = value;
            return found;
        }
    }
}
