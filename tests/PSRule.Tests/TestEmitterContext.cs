// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Data;
using PSRule.Options;
using PSRule.Pipeline.Emitters;

namespace PSRule;

internal sealed class TestEmitterContext(InputFormat format = InputFormat.None, string objectPath = null)
    : BaseEmitterContext(format, objectPath, false)
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
