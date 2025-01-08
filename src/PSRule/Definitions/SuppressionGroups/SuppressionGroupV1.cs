// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// A suppression group resource V1.
/// </summary>
[Spec(Specs.V1, Specs.SuppressionGroup)]
internal sealed class SuppressionGroupV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SuppressionGroupV1Spec spec)
    : InternalResource<SuppressionGroupV1Spec>(ResourceKind.SuppressionGroup, apiVersion, source, metadata, info, extent, spec), ISuppressionGroup
{
    ISuppressionGroupSpec IResource<ISuppressionGroupSpec>.Spec => Spec;
}
