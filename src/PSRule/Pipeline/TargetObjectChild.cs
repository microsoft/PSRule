// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Data;

namespace PSRule.Pipeline;

internal sealed class TargetObjectChild(object o, string? path = null) : ITargetObject
{
    public IEnumerable<TargetSourceInfo> Source => [];

    public ITargetSourceMap? SourceMap => null;

    public string[]? Scope => null;

    public string? Name => null;

    public string? Type => null;

    public string? Path => path;

    public object Value { get; } = o;

    public TAnnotation? GetAnnotation<TAnnotation>() where TAnnotation : class
    {
        return null;
    }

    public Hashtable? GetData() => null;

    public void SetAnnotation<TAnnotation>(TAnnotation value) where TAnnotation : class
    {
        // No-op
    }
}
