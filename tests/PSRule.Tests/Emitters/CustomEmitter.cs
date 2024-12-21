// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule.Emitters;

#nullable enable

internal sealed class CustomEmitter : IEmitter
{
    public bool Accepts(IEmitterContext context, Type type)
    {
        return true;
    }

    public void Dispose()
    {
        // Do nothing. Nothing to dispose.
    }

    public bool Visit(IEmitterContext context, object o)
    {
        throw new NotImplementedException();
    }
}

#nullable restore
