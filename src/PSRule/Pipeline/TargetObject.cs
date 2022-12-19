// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
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

    /// <summary>
    /// An object processed by PSRule.
    /// </summary>
    public sealed class TargetObject
    {
        private readonly Dictionary<Type, TargetObjectAnnotation> _Annotations;

        private Hashtable _Data;

        internal TargetObject(PSObject o)
            : this(o, null) { }

        internal TargetObject(PSObject o, TargetSourceCollection source)
        {
            o.ConvertTargetInfoProperty();
            o.ConvertTargetInfoType();
            Source = ReadSourceInfo(o, source);
            Issue = ReadIssueInfo(o, null);
            TargetName = o.GetTargetName();
            TargetType = o.GetTargetType();
            Scope = o.GetScope();
            Path = ReadPath(o);
            Value = Convert(o);
            _Annotations = new Dictionary<Type, TargetObjectAnnotation>();
        }

        internal TargetObject(PSObject o, string targetName = null, string targetType = null, string[] scope = null)
            : this(o, null)
        {
            if (targetName != null)
                TargetName = targetName;

            if (targetType != null)
                TargetType = targetType;

            if (scope != null && scope.Length > 0)
                Scope = scope;
        }

        internal PSObject Value { get; }

        internal TargetSourceCollection Source { get; private set; }

        internal TargetIssueCollection Issue { get; private set; }

        internal string TargetName { get; }

        internal string TargetType { get; }

        internal string[] Scope { get; }

        internal string Path { get; }

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

        private static string ReadPath(PSObject o)
        {
            return o.GetTargetPath();
        }

        private static TargetSourceCollection ReadSourceInfo(PSObject o, TargetSourceCollection source)
        {
            var result = source ?? new TargetSourceCollection();
            if (ExpressionHelpers.GetBaseObject(o) is ITargetInfo targetInfo)
                result.Add(targetInfo.Source);

            result.AddRange(o.GetSourceInfo());
            return result;
        }

        private static TargetIssueCollection ReadIssueInfo(PSObject o, TargetIssueCollection issue)
        {
            var result = issue ?? new TargetIssueCollection();
            result.AddRange(o.GetIssueInfo());
            return result;
        }

        private static PSObject Convert(PSObject o)
        {
            return o.BaseObject is JToken token ? JsonHelper.ToPSObject(token) : o;
        }
    }
}
