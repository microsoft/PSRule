// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline
{
    /// <summary>
    /// A comparer to sort <see cref="OptionScope"/> based on precedence.
    /// </summary>
    internal sealed class OptionScopeComparer : IComparer<OptionScope>
    {
        public int Compare(OptionScope x, OptionScope y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            if (x.Type == y.Type) return 0;
            return x.Type < y.Type ? -1 : 1;
        }
    }
}
