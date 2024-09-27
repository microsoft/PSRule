// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime.Binding;

internal sealed class ImmutableHashtable : Hashtable
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
