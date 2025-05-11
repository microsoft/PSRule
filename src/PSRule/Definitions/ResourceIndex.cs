// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class ResourceIndex(IEnumerable<IResource> items)
{
    private readonly IndexEntry[] _Items = Load(items);

    internal sealed class IndexEntry(ResourceId id, ResourceId target)
    {
        public readonly ResourceId Id = id;
        public readonly ResourceId Target = target;
    }

    public bool TryFind(string id, out ResourceId value, out ResourceIdKind kind)
    {
        value = default;
        kind = default;
        for (var i = 0; i < _Items.Length; i++)
        {
            if (_Items[i].Id.Equals(id))
            {
                value = _Items[i].Target;
                kind = _Items[i].Id.Kind;
                return true;
            }
        }
        return false;
    }

    private static IndexEntry[] Load(IEnumerable<IResource> items)
    {
        var results = new List<IndexEntry>();
        foreach (var item in items)
        {
            var ids = item.GetIds();
            foreach (var id in ids)
                results.Add(new IndexEntry(id, item.Id));
        }
        return results.ToArray();
    }

    public bool IsEmpty()
    {
        return _Items == null || _Items.Length == 0;
    }

    public IndexEntry[] GetItems()
    {
        return _Items;
    }
}
