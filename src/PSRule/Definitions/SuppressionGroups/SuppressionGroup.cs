// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Pipeline;

namespace PSRule.Definitions.SuppressionGroups
{
    /// <summary>
    /// A specification for a V1 suppression group resource.
    /// </summary>
    internal interface ISuppressionGroupV1Spec
    {
        string[] Rule { get; }

        LanguageIf If { get; }
    }

    /// <summary>
    /// A suppression group resource V1.
    /// </summary>
    [Spec(Specs.V1, Specs.SuppressionGroup)]
    internal sealed class SuppressionGroupV1 : InternalResource<SuppressionGroupV1Spec>
    {
        public SuppressionGroupV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SuppressionGroupV1Spec spec)
            : base(ResourceKind.SuppressionGroup, apiVersion, source, metadata, info, extent, spec) { }
    }

    /// <summary>
    /// A specification for a V1 suppression group resource.
    /// </summary>
    internal sealed class SuppressionGroupV1Spec : Spec, ISuppressionGroupV1Spec
    {
        public string[] Rule { get; set; }

        public LanguageIf If { get; set; }
    }
}
