// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

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
            if (object.Equals(x, null) || object.Equals(y, null))
                return object.Equals(x, y);

            return x.Equals(y);
        }

        public int GetHashCode(ISuppressionInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
