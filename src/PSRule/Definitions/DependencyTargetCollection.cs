// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class DependencyTargetCollection<T> where T : IDependencyTarget
{
    private readonly Dictionary<ResourceId, TargetLink> _Index;
    private readonly List<T> _Items;

    public DependencyTargetCollection()
    {
        _Items = [];
        _Index = new Dictionary<ResourceId, TargetLink>(ResourceIdEqualityComparer.Default);
    }

    private sealed class TargetLink(T link, ResourceIdKind kind)
    {
        public readonly T Link = link;
        public readonly ResourceIdKind Kind = kind;
    }

    public bool Contains(ResourceId id)
    {
        return _Index.ContainsKey(id);
    }

    public bool TryGet(ResourceId id, out T? value, out ResourceIdKind kind)
    {
        value = default;
        kind = default;
        if (!_Index.TryGetValue(id, out var link))
            return false;

        value = link.Link;
        kind = link.Kind;
        return true;
    }

    public bool TryAdd(T target)
    {
        if (_Index.ContainsKey(target.Id) || (target.Ref.HasValue && _Index.ContainsKey(target.Ref.Value)))
            return false;

        for (var i = 0; target.Alias != null && i < target.Alias.Length; i++)
            if (_Index.ContainsKey(target.Alias[i]))
                return false;

        // Add Id, Ref, and aliases to the index.
        _Index.Add(target.Id, new TargetLink(target, ResourceIdKind.Id));
        if (target.Ref.HasValue && target.Id != target.Ref.Value)
            _Index.Add(target.Ref.Value, new TargetLink(target, ResourceIdKind.Ref));

        for (var i = 0; target.Alias != null && i < target.Alias.Length; i++)
            _Index.Add(target.Alias[i], new TargetLink(target, ResourceIdKind.Alias));

        _Items.Add(target);
        return true;
    }

    public IEnumerable<T> GetAll()
    {
        return _Items;
    }
}
