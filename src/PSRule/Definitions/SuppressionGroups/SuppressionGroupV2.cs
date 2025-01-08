// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// A suppression group resource V2.
/// </summary>
[Spec(Specs.API_2025_01_01, Specs.SuppressionGroup)]
internal sealed class SuppressionGroupV2(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SuppressionGroupV2Spec spec)
    : InternalResource<SuppressionGroupV2Spec>(ResourceKind.SuppressionGroup, apiVersion, source, metadata, info, extent, spec), ISuppressionGroup
{
    ISuppressionGroupSpec IResource<ISuppressionGroupSpec>.Spec => Spec;
}
