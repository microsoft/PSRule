// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

/// <summary>
/// Describes an issue with a resource.
/// </summary>
/// <param name="type">The type of issue.</param>
/// <param name="resourceId">The affected resource by ID.</param>
/// <param name="args">Additional information based on the issue type.</param>
internal sealed class ResourceIssue(ResourceIssueType type, ResourceId resourceId, params object[]? args)
{
    /// <summary>
    /// The affected resource by ID.
    /// </summary>
    public ResourceId ResourceId { get; } = resourceId;

    /// <summary>
    /// The type of issue.
    /// </summary>
    public ResourceIssueType Type { get; } = type;

    /// <summary>
    /// Additional information based on the issue type.
    /// </summary>
    public object[]? Args { get; } = args;
}
