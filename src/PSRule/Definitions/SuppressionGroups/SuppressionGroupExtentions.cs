// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.SuppressionGroups;

#nullable enable

/// <summary>
/// Extensions methods for suppression groups.
/// </summary>
internal static class SuppressionGroupExtensions
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
}

#nullable restore
