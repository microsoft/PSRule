// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Pipeline;

namespace PSRule.Definitions.SuppressionGroups
{
    [Spec(Specs.V1, Specs.SuppressionGroup)]
    internal sealed class SuppressionGroupV1 : InternalResource<SuppressionGroupV1Spec>
    {
        public SuppressionGroupV1(string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, SuppressionGroupV1Spec spec)
            : base(ResourceKind.SuppressionGroup, apiVersion, source, metadata, info, spec) { }
    }

    internal interface ISuppressionGroupSpec
    {
        string[] Rule { get; }

        LanguageIf If { get; }
    }

    internal sealed class SuppressionGroupV1Spec : Spec, ISuppressionGroupSpec
    {
        public string[] Rule { get; set; }

        public LanguageIf If { get; set; }
    }
}