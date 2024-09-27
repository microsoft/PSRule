// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions.Baselines;

internal sealed class BaselineRef : ResourceRef
{
    public readonly ScopeType Type;

    public BaselineRef(string id, ScopeType scopeType)
        : base(id, ResourceKind.Baseline)
    {
        Type = scopeType;
    }
}
