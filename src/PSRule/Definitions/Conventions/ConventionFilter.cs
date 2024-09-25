// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Resources;

namespace PSRule.Definitions.Conventions;

internal sealed class ConventionFilter : IResourceFilter
{
    private readonly HashSet<string> _Include;
    private readonly WildcardPattern _WildcardMatch;

    public ConventionFilter(string[] include)
    {
        _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
        _WildcardMatch = null;
        if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
        {
            if (include.Length > 1)
                throw new NotSupportedException(PSRuleResources.MatchSingleName);

            _WildcardMatch = new WildcardPattern(include[0]);
        }
    }

    ResourceKind IResourceFilter.Kind => ResourceKind.Convention;

    public bool Match(IResource resource)
    {
        return _Include != null &&
            (_Include.Contains(resource.Name) ||
             _Include.Contains(resource.Id.Value) ||
             MatchWildcard(resource.Name) ||
             MatchWildcard(resource.Id.Value));
    }

    private bool MatchWildcard(string name)
    {
        return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
    }
}
