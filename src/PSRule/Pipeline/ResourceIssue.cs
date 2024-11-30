// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

internal sealed class ResourceIssue
{
    public ResourceIssue(IResource resource, ResourceIssueType issue)
    {
        Resource = resource;
        Issue = issue;
    }

    public IResource Resource { get; }

    public ResourceIssueType Issue { get; }
}
