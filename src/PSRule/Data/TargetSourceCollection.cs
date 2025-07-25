// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

internal sealed class TargetSourceCollection : ITargetSourceCollection
{
    private List<TargetSourceInfo> _Items;
    private Dictionary<string, TargetSourceInfo> _Index;

    internal TargetSourceCollection() { }

    internal TargetSourceCollection(IEnumerable<TargetSourceInfo> sourceInfo)
    {
        AddRange(sourceInfo);
    }

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
        return _Items == null || _Items.Count == 0 ? [] : [.. _Items];
    }

    internal void AddRange(IEnumerable<TargetSourceInfo> sourceInfo)
    {
        foreach (var i in sourceInfo)
            Add(i);
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
