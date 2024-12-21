// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Selectors;

namespace PSRule.Pipeline;

internal sealed class SelectorTargetAnnotation : TargetObjectAnnotation
{
    private readonly Dictionary<Guid, bool> _Results;

    public SelectorTargetAnnotation()
    {
        _Results = new Dictionary<Guid, bool>();
    }

    public bool TryGetSelectorResult(SelectorVisitor selector, out bool result)
    {
        return _Results.TryGetValue(selector.InstanceId, out result);
    }

    public void SetSelectorResult(SelectorVisitor selector, bool result)
    {
        _Results[selector.InstanceId] = result;
    }
}
