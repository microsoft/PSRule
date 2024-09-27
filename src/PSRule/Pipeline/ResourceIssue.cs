// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

internal sealed class ResourceIssue
{
    public ResourceIssue(ResourceKind resourceKind, ResourceId resourceId, ResourceIssueType issue)
    {
        ResourceKind = resourceKind;
        ResourceId = resourceId;
        Issue = issue;
    }

    public ResourceKind ResourceKind { get; }

    public ResourceId ResourceId { get; }

    public ResourceIssueType Issue { get; }
}
