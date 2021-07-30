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
                if (_Index == null || !_Index.TryGetValue(type, out TargetSourceInfo value))
                    return null;

                return value;
            }
        }

        internal TargetSourceInfo[] GetSourceInfo()
        {
            if (_Items == null)
                return Array.Empty<TargetSourceInfo>();

            return _Items.ToArray();
        }

        internal void AddRange(TargetSourceInfo[] sourceInfo)
        {
            for (var i = 0; sourceInfo != null && i < sourceInfo.Length; i++)
                Add(sourceInfo[i]);
        }

        private void Add(TargetSourceInfo sourceInfo)
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
