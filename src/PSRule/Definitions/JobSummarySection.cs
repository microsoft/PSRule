// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Represents a section of content to be included in a job summary.
/// </summary>
/// <param name="title">The title for the section (will be rendered as H2 header).</param>
/// <param name="content">The content for the section.</param>
public sealed class JobSummarySection(string title, InfoString content)
{
    /// <summary>
    /// Gets the title for this section.
    /// </summary>
    public string Title { get; } = title ?? throw new ArgumentNullException(nameof(title));

    /// <summary>
    /// Gets the content for this section.
    /// </summary>
    public InfoString Content { get; } = content ?? throw new ArgumentNullException(nameof(content));
}
