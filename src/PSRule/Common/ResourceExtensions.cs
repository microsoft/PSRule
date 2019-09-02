﻿using PSRule.Rules;

namespace PSRule
{
    internal static class ResourceExtensions
    {
        internal static bool Match(this IResourceFilter filter, Baseline resource)
        {
            return filter.Match(resource.Name, null);
        }
    }
}
