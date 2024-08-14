// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal interface IAnnotated<T>
{
    TAnnotation GetAnnotation<TAnnotation>() where TAnnotation : T;

    void SetAnnotation<TAnnotation>(TAnnotation annotation) where TAnnotation : T;
}
