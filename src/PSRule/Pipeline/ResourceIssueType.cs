// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

internal enum ResourceIssueType
{
    Unknown,

    SuppressionGroupExpired,

    DuplicateResourceId,

    DuplicateResourceName,
}
