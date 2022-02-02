// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace PSRule.Data
{
    public interface ITargetSourceCollection
    {
        TargetSourceInfo this[string type] { get; }
    }

    internal sealed class TargetSourceCollection : ITargetSourceCollection
    {
        private List<TargetSourceInfo> _Items;
        private Dictionary<string, TargetSourceInfo> _Index;

        internal TargetSourceCollection() { }

        public TargetSourceInfo this[string type]
        {
            get
            {
                return _Index == null || !_Index.TryGetValue(type, out var value) ? null : value;
            }
        }

        internal TargetSourceInfo[] GetSourceInfo()
        {
            return _Items == null ? Array.Empty<TargetSourceInfo>() : _Items.ToArray();
        }

        internal void AddRange(TargetSourceInfo[] sourceInfo)
        {
            for (var i = 0; sourceInfo != null && i < sourceInfo.Length; i++)
                Add(sourceInfo[i]);
        }

        internal void Add(TargetSourceInfo sourceInfo)
        {
            if (sourceInfo == null)
                return;

            if (_Index == null && !string.IsNullOrEmpty(sourceInfo.Type))
                _Index = new Dictionary<string, TargetSourceInfo>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(sourceInfo.Type) || _Index.ContainsKey(sourceInfo.Type))
                return;

            if (_Items == null)
                _Items = new List<TargetSourceInfo>();

            _Items.Add(sourceInfo);
            _Index.Add(sourceInfo.Type, sourceInfo);
        }
    }
}
