// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// Filter out suppression groups that have already expired.
/// </summary>
internal sealed class SuppressionGroupFilter : ResourceFilter<ISuppressionGroup>
{
    public override ResourceKind Kind => ResourceKind.SuppressionGroup;

    public override bool Match(ISuppressionGroup resource)
    {
        switch (resource.Spec)
        {
            case ISuppressionGroupV1Spec v1:
                if (!v1.ExpiresOn.HasValue || v1.ExpiresOn.Value > DateTime.UtcNow)
                {
                    return true;
                }
                break;

            case ISuppressionGroupV2Spec v2:
                if (!v2.ExpiresOn.HasValue || v2.ExpiresOn.Value > DateTime.UtcNow)
                {
                    return true;
                }
                break;
        }
        return false;
    }
}
