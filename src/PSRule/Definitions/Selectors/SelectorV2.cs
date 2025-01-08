// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions.Selectors;

/// <summary>
/// A selector resource V2.
/// </summary>
[Spec(Specs.API_2025_01_01, Specs.Selector)]
internal sealed class SelectorV2(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SelectorV2Spec spec)
    : InternalResource<SelectorV2Spec>(ResourceKind.Selector, apiVersion, source, metadata, info, extent, spec), ISelector
{
    ISelectorSpec IResource<ISelectorSpec>.Spec => Spec;
}
