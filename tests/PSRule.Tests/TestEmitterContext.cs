// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Data;
using PSRule.Emitters;

namespace PSRule;

#nullable enable

internal sealed class TestEmitterContext(string? stringFormat = default, string? objectPath = default)
    : BaseEmitterContext(stringFormat, objectPath, false)
{
    public List<ITargetObject> Items = [];

    protected override void Enqueue(ITargetObject value)
    {
        Items.Add(value);
    }

    public override bool ShouldQueue(string path)
    {
        return true;
    }
}

#nullable restore
