// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions.Selectors;

/// <summary>
/// A selector resource V1.
/// </summary>
[Spec(Specs.V1, Specs.Selector)]
internal sealed class SelectorV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SelectorV1Spec spec)
    : InternalResource<SelectorV1Spec>(ResourceKind.Selector, apiVersion, source, metadata, info, extent, spec), ISelector
{
    ISelectorSpec IResource<ISelectorSpec>.Spec => Spec;
}
