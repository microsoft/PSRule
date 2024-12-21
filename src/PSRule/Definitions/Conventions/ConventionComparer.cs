// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Conventions;

/// <summary>
/// Orders conventions by the order they are specified.
/// </summary>
internal sealed class ConventionComparer : IComparer<IConventionV1>
{
    private readonly Func<IConventionV1, int> _GetOrder;

    internal ConventionComparer(Func<IConventionV1, int> getOrder)
    {
        _GetOrder = getOrder;
    }

    public int Compare(IConventionV1 x, IConventionV1 y)
    {
        return _GetOrder(x) - _GetOrder(y);
    }
}
