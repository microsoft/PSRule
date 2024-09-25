// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal static class AnnotatedExtensions
{
    internal static TAnnotation RequireAnnotation<T, TAnnotation>(this IAnnotated<T> annotated) where TAnnotation : T, new()
    {
        var result = annotated.GetAnnotation<TAnnotation>();
        return result == null ? new TAnnotation() : result;
    }
}
