// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule
{
    internal static class ResourceExtensions
    {
        internal static void SetApiVersionIssue(this IResource resource)
        {
            if (!(resource is IAnnotated<ResourceAnnotation> annotated))
                return;

            var validate = annotated.RequireAnnotation<ResourceAnnotation, ValidateResourceAnnotation>();
            validate.ApiVersionNotSet = true;
            annotated.SetAnnotation(validate);
        }

        internal static bool GetApiVersionIssue(this IResource resource)
        {
            if (!(resource is IAnnotated<ResourceAnnotation> annotated))
                return false;

            var validate = annotated.GetAnnotation<ValidateResourceAnnotation>();
            return validate != null && validate.ApiVersionNotSet;
        }

        internal static bool Match(this IResourceFilter filter, Baseline resource)
        {
            return filter.Match(resource.Name, null);
        }
    }
}
