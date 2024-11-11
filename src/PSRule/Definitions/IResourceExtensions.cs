// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Runtime;

namespace PSRule.Definitions;

#nullable enable

/// <summary>
/// Extensions for resource types.
/// </summary>
internal static class IResourceExtensions
{
    /// <summary>
    /// Convert a suppression group into a suppression group visitor.
    /// </summary>
    /// <param name="resource">The suppression group resource.</param>
    /// <param name="runspaceContext">A valid runspace context.</param>
    /// <returns>An instance of a <see cref="SuppressionGroupVisitor"/>.</returns>
    public static SuppressionGroupVisitor ToSuppressionGroupVisitor(this SuppressionGroupV1 resource, RunspaceContext runspaceContext)
    {
        return new SuppressionGroupVisitor(
            context: runspaceContext,
            id: resource.Id,
            source: resource.Source,
            spec: resource.Spec,
            info: resource.Info
        );
    }

    /// <summary>
    /// Converts a selector into a selector visitor.
    /// </summary>
    /// <param name="resource">The selector resource.</param>
    /// <param name="runspaceContext">A valid runspace context.</param>
    /// <returns>An instance of a <see cref="SelectorVisitor"/>.</returns>
    public static SelectorVisitor ToSelectorVisitor(this SelectorV1 resource, RunspaceContext runspaceContext)
    {
        return new SelectorVisitor(
            runspaceContext,
            resource.Id,
            resource.Source,
            resource.Spec.If
        );
    }
}

#nullable restore
