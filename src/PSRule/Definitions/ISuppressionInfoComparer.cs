// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class ISuppressionInfoComparer : IEqualityComparer<ISuppressionInfo>
{
    public bool Equals(ISuppressionInfo x, ISuppressionInfo y)
    {
        return object.Equals(x, null) || object.Equals(y, null) ? object.Equals(x, y) : x.Equals(y);
    }

    public int GetHashCode(ISuppressionInfo obj)
    {
        return obj.GetHashCode();
    }
}
