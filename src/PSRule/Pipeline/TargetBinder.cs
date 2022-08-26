// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using PSRule.Configuration;
using static PSRule.Pipeline.TargetBinder;

namespace PSRule.Pipeline
{
    /// <summary>
    /// Responsible for handling binding for a given target object.
    /// </summary>
    internal interface ITargetBinder
    {
        void Bind(TargetObject targetObject);

        ITargetBindingContext Using(string languageScope);

        ITargetBindingResult Result(string languageScope);
    }

    /// <summary>
    /// A binding context specific to a language scope.
    /// </summary>
    internal interface ITargetBindingContext
    {
        string LanguageScope { get; }

        ITargetBindingResult Bind(object o);
    }

    internal interface ITargetBindingResult
    {
        /// <summary>
        /// The bound TargetName of the target object.
        /// </summary>
        string TargetName { get; }

        string TargetNamePath { get; }

        /// <summary>
        /// The bound TargetType of the target object.
        /// </summary>
        string TargetType { get; }

        string TargetTypePath { get; }

        /// <summary>
        /// Additional bound fields of the target object.
        /// </summary>
        Hashtable Field { get; }

        bool ShouldFilter { get; }
    }

    /// <summary>
    /// Builds a TargetBinder.
    /// </summary>
    internal sealed class TargetBinderBuilder
    {
        private readonly List<ITargetBindingContext> _BindingContext;
        private readonly HashSet<string> _TypeFilter;
        private readonly BindTargetMethod _BindTargetName;
        private readonly BindTargetMethod _BindTargetType;
        private readonly BindTargetMethod _BindField;

        public TargetBinderBuilder(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, string[] typeFilter)
        {
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            _BindField = bindField;
            _BindingContext = new List<ITargetBindingContext>();
            if (typeFilter != null && typeFilter.Length > 0)
                _TypeFilter = new HashSet<string>(typeFilter, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Build a TargetBinder.
        /// </summary>
        public ITargetBinder Build()
        {
            return new TargetBinder(_BindingContext.ToArray());
        }

        /// <summary>
        /// Add a target binding context.
        /// </summary>
        public void With(ITargetBindingContext bindingContext)
        {
            _BindingContext.Add(bindingContext);
        }

        /// <summary>
        /// Add a target binding context.
        /// </summary>
        public void With(string languageScope, IBindingOption bindingOption)
        {
            _BindingContext.Add(new TargetBindingContext(languageScope, bindingOption, _BindTargetName, _BindTargetType, _BindField, _TypeFilter));
        }
    }

    /// <summary>
    /// Responsible for handling binding for a given target object.
    /// </summary>
    internal sealed class TargetBinder : ITargetBinder
    {
        private const string STANDALONE_SCOPE = ".";

        private readonly Dictionary<string, ITargetBindingContext> _BindingContext;
        private readonly Dictionary<string, ITargetBindingResult> _BindingResult;

        internal TargetBinder(ITargetBindingContext[] bindingContext)
        {
            _BindingContext = new Dictionary<string, ITargetBindingContext>();
            _BindingResult = new Dictionary<string, ITargetBindingResult>();
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

        internal sealed class TargetBindingResult : ITargetBindingResult
        {
            public TargetBindingResult(string targetName, string targetNamePath, string targetType, string targetTypePath, bool shouldFilter, Hashtable field)
            {
                TargetName = targetName;
                TargetNamePath = targetNamePath;
                TargetType = targetType;
                TargetTypePath = targetTypePath;
                ShouldFilter = shouldFilter;
                Field = field;
            }

            /// <inheritdoc/>
            public string TargetName { get; }

            /// <inheritdoc/>
            public string TargetNamePath { get; }

            /// <inheritdoc/>
            public string TargetType { get; }

            /// <inheritdoc/>
            public string TargetTypePath { get; }

            /// <inheritdoc/>
            public bool ShouldFilter { get; }

            /// <inheritdoc/>
            public Hashtable Field { get; }
        }

        internal sealed class TargetBindingContext : ITargetBindingContext
        {
            private readonly IBindingOption _BindingOption;
            private readonly BindTargetMethod _BindTargetName;
            private readonly BindTargetMethod _BindTargetType;
            private readonly BindTargetMethod _BindField;
            private readonly HashSet<string> _TypeFilter;

            public TargetBindingContext(string languageScope, IBindingOption bindingOption, BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField, HashSet<string> typeFilter)
            {
                LanguageScope = languageScope;
                _BindingOption = bindingOption;
                _BindTargetName = bindTargetName;
                _BindTargetType = bindTargetType;
                _BindField = bindField;
                _TypeFilter = typeFilter;
            }

            public string LanguageScope { get; }

            public ITargetBindingResult Bind(object o)
            {
                var targetName = _BindTargetName(_BindingOption.TargetName, !_BindingOption.IgnoreCase, _BindingOption.PreferTargetInfo, o, out var targetNamePath);
                var targetType = _BindTargetType(_BindingOption.TargetType, !_BindingOption.IgnoreCase, _BindingOption.PreferTargetInfo, o, out var targetTypePath);
                var shouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(targetType));

                // Use qualified name
                if (_BindingOption.UseQualifiedName)
                    targetName = string.Concat(targetType, _BindingOption.NameSeparator, targetName);

                // Bind custom fields
                var field = BindField(_BindField, _BindingOption.Field, !_BindingOption.IgnoreCase, o);

                return new TargetBindingResult
                (
                    targetName: targetName,
                    targetNamePath: targetNamePath,
                    targetType: targetType,
                    targetTypePath: targetTypePath,
                    shouldFilter: shouldFilter,
                    field: field
                );
            }
        }

        public bool ShouldFilter { get; private set; }

        /// <summary>
        /// Bind target object based on the supplied baseline.
        /// </summary>
        public void Bind(TargetObject targetObject)
        {
            foreach (var bindingContext in _BindingContext.Values)
                _BindingResult[bindingContext.LanguageScope] = bindingContext.Bind(targetObject.Value);
        }

        public ITargetBindingContext Using(string languageScope)
        {
            return _BindingContext.TryGetValue(languageScope ?? STANDALONE_SCOPE, out var result) ? result : null;
        }

        public ITargetBindingResult Result(string languageScope)
        {
            return _BindingResult.TryGetValue(languageScope ?? STANDALONE_SCOPE, out var result) ? result : null;
        }

        /// <summary>
        /// Bind additional fields.
        /// </summary>
        private static Hashtable BindField(BindTargetMethod bindField, FieldMap[] map, bool caseSensitive, object o)
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

                    hashtable.Add(field.Key, bindField(field.Value, caseSensitive, false, o, out _));
                }
            }
            hashtable.Protect();
            return hashtable;
        }
    }
}
