// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSRule.Configuration;

namespace PSRule.Pipeline
{
    /// <summary>
    /// Responsible for handling binding for a given target object.
    /// </summary>
    internal interface ITargetBinder
    {
        void Bind(TargetObject targetObject);

        ITargetBindingContext Using(string languageScope);
    }

    /// <summary>
    /// A binding context specific to a language scope.
    /// </summary>
    internal interface ITargetBindingContext
    {
        string LanguageScope { get; }

        /// <summary>
        /// The bound TargetName of the target object.
        /// </summary>
        string TargetName { get; }

        /// <summary>
        /// The bound TargetType of the target object.
        /// </summary>
        string TargetType { get; }

        /// <summary>
        /// Additional bound fields of the target object.
        /// </summary>
        Hashtable Field { get; }

        bool ShouldFilter { get; }

        void Bind(TargetObject targetObject, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, HashSet<string> typeFilter);
    }

    /// <summary>
    /// Builds a TargetBinder.
    /// </summary>
    internal sealed class TargetBinderBuilder
    {
        private readonly List<ITargetBindingContext> _BindingContext;
        private readonly string[] _TypeFilter;
        private readonly BindTargetMethod _BindTargetName;
        private readonly BindTargetMethod _BindTargetType;
        private readonly BindTargetMethod _BindField;

        public TargetBinderBuilder(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, string[] typeFilter)
        {
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            _BindField = bindField;
            _BindingContext = new List<ITargetBindingContext>();
            _TypeFilter = typeFilter;
        }

        /// <summary>
        /// Build a TargetBinder.
        /// </summary>
        public ITargetBinder Build()
        {
            return new TargetBinder(_BindingContext.ToArray(), _BindTargetName, _BindTargetType, _BindField, _TypeFilter);
        }

        /// <summary>
        /// Add a target binding context.
        /// </summary>
        public void With(ITargetBindingContext bindingContext)
        {
            _BindingContext.Add(bindingContext);
        }
    }

    /// <summary>
    /// Responsible for handling binding for a given target object.
    /// </summary>
    internal sealed class TargetBinder : ITargetBinder
    {
        private const string STANDALONE_SCOPE = ".";

        private readonly BindTargetMethod _BindTargetName;

        private readonly BindTargetMethod _BindTargetType;
        private readonly BindTargetMethod _BindField;
        private readonly HashSet<string> _TypeFilter;

        private readonly Dictionary<string, ITargetBindingContext> _BindingContext;

        internal TargetBinder(ITargetBindingContext[] bindingContext, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, string[] typeFilter)
        {
            _BindingContext = new Dictionary<string, ITargetBindingContext>();
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            _BindField = bindField;
            if (typeFilter != null && typeFilter.Length > 0)
                _TypeFilter = new HashSet<string>(typeFilter, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; bindingContext != null && i < bindingContext.Length; i++)
                _BindingContext.Add(bindingContext[i].LanguageScope ?? STANDALONE_SCOPE, bindingContext[i]);
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

        internal sealed class TargetBindingContext : ITargetBindingContext
        {
            private readonly IBindingOption _BindingOption;

            public TargetBindingContext(string languageScope, IBindingOption bindingOption)
            {
                LanguageScope = languageScope;
                _BindingOption = bindingOption;
            }

            public string LanguageScope { get; }

            /// <summary>
            /// The bound TargetName of the target object.
            /// </summary>
            public string TargetName { get; private set; }

            /// <summary>
            /// The bound TargetType of the target object.
            /// </summary>
            public string TargetType { get; private set; }

            /// <summary>
            /// Additional bound fields of the target object.
            /// </summary>
            public Hashtable Field { get; private set; }

            /// <summary>
            /// Determines if the target object should be filtered.
            /// </summary>
            public bool ShouldFilter { get; private set; }

            public void Bind(TargetObject targetObject, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, HashSet<string> typeFilter)
            {
                //_BindingOption.

                TargetName = bindTargetName(_BindingOption.TargetName, !_BindingOption.IgnoreCase, _BindingOption.PreferTargetInfo, targetObject.Value);
                TargetType = bindTargetType(_BindingOption.TargetType, !_BindingOption.IgnoreCase, _BindingOption.PreferTargetInfo, targetObject.Value);
                ShouldFilter = !(typeFilter == null || typeFilter.Contains(TargetType));

                // Use qualified name
                if (_BindingOption.UseQualifiedName)
                    TargetName = string.Concat(TargetType, _BindingOption.NameSeparator, TargetName);

                // Bind custom fields
                Field = BindField(bindField, _BindingOption.Field, !_BindingOption.IgnoreCase, targetObject.Value);
            }
        }


        public bool ShouldFilter { get; private set; }


        //public void Bind(OptionContext baseline, TargetObject targetObject)
        //{
        //    var binding = baseline.GetTargetBinding();
        //    TargetName = _BindTargetName(binding.TargetName, !binding.IgnoreCase, binding.PreferTargetInfo, targetObject.Value);
        //    TargetType = _BindTargetType(binding.TargetType, !binding.IgnoreCase, binding.PreferTargetInfo, targetObject.Value);
        //    ShouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(TargetType));

        //    // Use qualified name
        //    if (binding.UseQualifiedName)
        //        TargetName = string.Concat(TargetType, binding.NameSeparator, TargetName);

        //    // Bind custom fields
        //    BindField(binding.Field, !binding.IgnoreCase, targetObject.Value);
        //}

        /// <summary>
        /// Bind target object based on the supplied baseline.
        /// </summary>
        public void Bind(TargetObject targetObject)
        {
            foreach (var bindingContext in _BindingContext.Values)
                bindingContext.Bind(targetObject, _BindTargetName, _BindTargetType, _BindField, _TypeFilter);
        }

        public ITargetBindingContext Using(string languageScope)
        {
            if (_BindingContext.TryGetValue(languageScope ?? STANDALONE_SCOPE, out ITargetBindingContext result))
                return result;

            return null;
        }

        /// <summary>
        /// Bind additional fields.
        /// </summary>
        private static Hashtable BindField(BindTargetMethod bindField, FieldMap[] map, bool caseSensitive, PSObject targetObject)
        {
            if (map == null || map.Length == 0)
                return null;

            var hashtable = new ImmutableHashtable();
            for (var i = 0; i < map.Length; i++)
            {
                if (map[i] == null || map[i].Count == 0)
                    continue;

                foreach (var field in map[i])
                {
                    if (hashtable.ContainsKey(field.Key))
                        continue;

                    hashtable.Add(field.Key, bindField(field.Value, caseSensitive, false, targetObject));
                }
            }
            hashtable.Protect();
            return hashtable;
        }
    }
}
