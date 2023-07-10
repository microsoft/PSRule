// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    /// <summary>
    /// Information related to suppression of a rule.
    /// </summary>
    internal interface ISuppressionInfo
    {
        ResourceId Id { get; }

        InfoString Synopsis { get; }

        int Count { get; }
    }

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
}
