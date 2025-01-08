// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class TargetObjectChild(object o, string? path = null) : ITargetObject
{
    public IEnumerable<TargetSourceInfo> Source => [];

    public ITargetSourceMap? SourceMap => null;

    public string? Name => null;

    public string? Type => null;

    public string? Path => path;

    public object Value { get; } = o;
}

#nullable restore
