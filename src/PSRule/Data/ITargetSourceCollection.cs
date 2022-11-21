// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace PSRule.Data
{
    /// <summary>
    /// A collection of sources for a target object.
    /// </summary>
    public interface ITargetSourceCollection
    {
        /// <summary>
        /// Get the source details by source type.
        /// </summary>
        TargetSourceInfo this[string type] { get; }
    }

    internal sealed class TargetSourceCollection : ITargetSourceCollection
    {
        private List<TargetSourceInfo> _Items;
        private Dictionary<string, TargetSourceInfo> _Index;

        internal TargetSourceCollection() { }

        public int Count => _Items == null ? 0 : _Items.Count;

        public TargetSourceInfo this[string type]
        {
            get
            {
                return _Index == null || _Index.Count == 0 || !_Index.TryGetValue(type, out var value) ? null : value;
            }
        }

        internal TargetSourceInfo[] GetSourceInfo()
        {
            return _Items == null || _Items.Count == 0 ? Array.Empty<TargetSourceInfo>() : _Items.ToArray();
        }

        internal void AddRange(TargetSourceInfo[] sourceInfo)
        {
            for (var i = 0; sourceInfo != null && i < sourceInfo.Length; i++)
                Add(sourceInfo[i]);
        }

        internal void Add(TargetSourceInfo sourceInfo)
        {
            if (sourceInfo == null || string.IsNullOrEmpty(sourceInfo.Type))
                return;

            _Index ??= new Dictionary<string, TargetSourceInfo>(StringComparer.OrdinalIgnoreCase);
            if (_Index.ContainsKey(sourceInfo.Type))
                return;

            _Items ??= new List<TargetSourceInfo>();
            _Items.Add(sourceInfo);
            _Index.Add(sourceInfo.Type, sourceInfo);
        }
    }
}
