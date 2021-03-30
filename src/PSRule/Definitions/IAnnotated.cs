// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    internal interface IAnnotated<T>
    {
        TAnnotation GetAnnotation<TAnnotation>() where TAnnotation : T;

        void SetAnnotation<TAnnotation>(TAnnotation annotation) where TAnnotation : T;
    }

    internal static class AnnotatedExtensions
    {
        internal static TAnnotation RequireAnnotation<T, TAnnotation>(this IAnnotated<T> annotated) where TAnnotation : T, new()
        {
            var result = annotated.GetAnnotation<TAnnotation>();
            return result == null ? new TAnnotation() : result;
        }
    }
}
