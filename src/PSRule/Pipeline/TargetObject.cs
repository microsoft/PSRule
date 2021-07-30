// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Definitions.Selectors;
using System;
using System.Collections.Generic;
using System.Management.Automation;

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

        internal TargetObject(PSObject o)
            : this(o, null) { }

        internal TargetObject(PSObject o, TargetSourceCollection source)
        {
            Value = o;
            Source = ReadSourceInfo(source);
            _Annotations = new Dictionary<Type, TargetObjectAnnotation>();
        }

        internal PSObject Value { get; }

        internal TargetSourceCollection Source { get; private set; }

        internal T GetAnnotation<T>() where T : TargetObjectAnnotation, new()
        {
            if (!_Annotations.TryGetValue(typeof(T), out TargetObjectAnnotation value))
            {
                value = new T();
                _Annotations.Add(typeof(T), value);
            }
            return (T)value;
        }

        private TargetSourceCollection ReadSourceInfo(TargetSourceCollection source)
        {
            var result = source == null ? new TargetSourceCollection() : source;
            Value.ConvertTargetInfoProperty();
            result.AddRange(Value.GetSourceInfo());
            return result;
        }
    }
}
