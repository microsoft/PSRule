// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Definitions.Selectors;

namespace PSRule.Pipeline
{
    internal abstract class TargetObjectAnnotation
    {

    }

    internal sealed class SelectorTargetAnnotation : TargetObjectAnnotation
    {
        private readonly Dictionary<Guid, bool> _Results;

        public SelectorTargetAnnotation()
        {
            _Results = new Dictionary<Guid, bool>();
        }

        public bool TryGetSelectorResult(SelectorVisitor selector, out bool result)
        {
            return _Results.TryGetValue(selector.InstanceId, out result);
        }

        public void SetSelectorResult(SelectorVisitor selector, bool result)
        {
            _Results[selector.InstanceId] = result;
        }
    }

    public sealed class TargetObject
    {
        private readonly Dictionary<Type, TargetObjectAnnotation> _Annotations;

        private Hashtable _Data;

        internal TargetObject(PSObject o)
            : this(o, null) { }

        internal TargetObject(PSObject o, TargetSourceCollection source)
        {
            Value = o;
            Source = ReadSourceInfo(source);
            Issue = ReadIssueInfo(null);
            _Annotations = new Dictionary<Type, TargetObjectAnnotation>();
        }

        internal PSObject Value { get; }

        internal TargetSourceCollection Source { get; private set; }

        internal TargetIssueCollection Issue { get; private set; }

        internal Hashtable GetData()
        {
            return _Data == null || _Data.Count == 0 ? null : _Data;
        }

        internal Hashtable RequireData()
        {
            _Data ??= new Hashtable();
            return _Data;
        }

        internal T GetAnnotation<T>() where T : TargetObjectAnnotation, new()
        {
            if (!_Annotations.TryGetValue(typeof(T), out var value))
            {
                value = new T();
                _Annotations.Add(typeof(T), value);
            }
            return (T)value;
        }

        private TargetSourceCollection ReadSourceInfo(TargetSourceCollection source)
        {
            var result = source ?? new TargetSourceCollection();
            if (ExpressionHelpers.GetBaseObject(Value) is ITargetInfo targetInfo)
                result.Add(targetInfo.Source);

            Value.ConvertTargetInfoProperty();
            result.AddRange(Value.GetSourceInfo());
            return result;
        }

        private TargetIssueCollection ReadIssueInfo(TargetIssueCollection issue)
        {
            var result = issue ?? new TargetIssueCollection();
            Value.ConvertTargetInfoProperty();
            result.AddRange(Value.GetIssueInfo());
            return result;
        }
    }
}
