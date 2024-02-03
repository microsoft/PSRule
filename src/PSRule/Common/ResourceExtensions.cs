// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Baselines;

namespace PSRule;

internal static class ResourceExtensions
{
    internal static bool TryValidateResourceAnnotation(this IResource resource, out ValidateResourceAnnotation value)
    {
        value = null;
        if (resource is not IAnnotated<ResourceAnnotation> annotated)
            return false;

        value = annotated.GetAnnotation<ValidateResourceAnnotation>();
        return value != null;
    }

    internal static bool Match(this IResourceFilter filter, Baseline resource)
    {
        return filter.Match(resource);
    }

    internal static bool IsLocalScope(this IResource resource)
    {
        return string.IsNullOrEmpty(resource.Source.Module);
    }

    internal static IEnumerable<ResourceId> GetIds(this IResource resource)
    {
        yield return resource.Id;
        if (resource.Ref.HasValue)
            yield return resource.Ref.Value;

        for (var i = 0; resource.Alias != null && i < resource.Alias.Length; i++)
            yield return resource.Alias[i];
    }
}
