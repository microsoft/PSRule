// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// Filter out suppression groups that have already expired.
/// </summary>
internal sealed class SuppressionGroupFilter : ResourceFilter<SuppressionGroupV1>
{
    public override ResourceKind Kind => ResourceKind.SuppressionGroup;

    public override bool Match(SuppressionGroupV1 resource)
    {
        return !resource.Spec.ExpiresOn.HasValue || resource.Spec.ExpiresOn.Value > DateTime.UtcNow;
    }
}
