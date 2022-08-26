// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Pipeline;

namespace PSRule.Definitions.Selectors
{
    /// <summary>
    /// A selector resource V1.
    /// </summary>
    [Spec(Specs.V1, Specs.Selector)]
    internal sealed class SelectorV1 : InternalResource<SelectorV1Spec>
    {
        public SelectorV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SelectorV1Spec spec)
            : base(ResourceKind.Selector, apiVersion, source, metadata, info, extent, spec) { }
    }

    /// <summary>
    /// A specification for a V1 selector resource.
    /// </summary>
    internal sealed class SelectorV1Spec : Spec
    {
        public LanguageIf If { get; set; }
    }
}
