// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class TargetBinder
    {
        private readonly BindTargetMethod _BindTargetName;
        private readonly BindTargetMethod _BindTargetType;
        private readonly BindTargetMethod _BindField;
        private readonly HashSet<string> _TypeFilter;

        internal TargetBinder(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, string[] typeFilter)
        {
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            _BindField = bindField;
            if (typeFilter != null && typeFilter.Length > 0)
                _TypeFilter = new HashSet<string>(typeFilter, StringComparer.OrdinalIgnoreCase);
        }

        private sealed class ImmutableHashtable : Hashtable
        {
            private bool _ReadOnly;

            internal ImmutableHashtable()
                : base(StringComparer.OrdinalIgnoreCase) { }

            public override bool IsReadOnly => _ReadOnly;

            public override void Add(object key, object value)
            {
                if (_ReadOnly)
                    throw new InvalidOperationException();

                base.Add(key, value);
            }

            public override void Clear()
            {
                if (_ReadOnly)
                    throw new InvalidOperationException();

                base.Clear();
            }

            public override void Remove(object key)
            {
                if (_ReadOnly)
                    throw new InvalidOperationException();

                base.Remove(key);
            }

            public override object this[object key]
            {
                get => base[key];
                set
                {
                    if (_ReadOnly)
                        throw new InvalidOperationException();

                    base[key] = value;
                }
            }

            internal void Protect()
            {
                _ReadOnly = true;
            }
        }

        /// <summary>
        /// Additional bound fields of the target object.
        /// </summary>
        public Hashtable Field { get; private set; }

        /// <summary>
        /// The bound TargetName of the target object.
        /// </summary>
        public string TargetName { get; private set; }

        /// <summary>
        /// The bound TargetType of the target object.
        /// </summary>
        public string TargetType { get; private set; }

        /// <summary>
        /// Determines if the target object should be filtered.
        /// </summary>
        public bool ShouldFilter { get; private set; }

        /// <summary>
        /// Bind target object based on the supplied baseline.
        /// </summary>
        public void Bind(OptionContext baseline, PSObject targetObject)
        {
            var binding = baseline.GetTargetBinding();
            TargetName = _BindTargetName(binding.TargetName, !binding.IgnoreCase, binding.PreferTargetInfo, targetObject);
            TargetType = _BindTargetType(binding.TargetType, !binding.IgnoreCase, binding.PreferTargetInfo, targetObject);
            ShouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(TargetType));

            // Use qualified name
            if (binding.UseQualifiedName)
                TargetName = string.Concat(TargetType, binding.NameSeparator, TargetName);

            // Bind custom fields
            BindField(binding.Field, !binding.IgnoreCase, targetObject);
        }

        /// <summary>
        /// Bind additional fields.
        /// </summary>
        private void BindField(FieldMap[] map, bool caseSensitive, PSObject targetObject)
        {
            if (map == null || map.Length == 0)
                return;

            var hashtable = new ImmutableHashtable();
            for (var i = 0; i < map.Length; i++)
            {
                if (map[i] == null || map[i].Count == 0)
                    continue;

                foreach (var field in map[i])
                {
                    if (hashtable.ContainsKey(field.Key))
                        continue;

                    hashtable.Add(field.Key, _BindField(field.Value, caseSensitive, false, targetObject));
                }
            }
            hashtable.Protect();
            Field = hashtable;
        }
    }
}
