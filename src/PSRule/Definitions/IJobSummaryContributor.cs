// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Defines an interface for conventions that can contribute additional information to job summaries.
/// </summary>
public interface IJobSummaryContributor
{
    /// <summary>
    /// Gets additional content to include in the job summary.
    /// This method is called during job summary generation to collect custom content from conventions.
    /// </summary>
    /// <returns>
    /// A collection of job summary sections, where each section contains a title and content that can be plain text or markdown.
    /// Returns null or empty collection if no additional content should be added.
    /// </returns>
    IEnumerable<JobSummarySection>? GetJobSummaryContent();
}

/// <summary>
/// Represents a section of content to be included in a job summary.
/// </summary>
public sealed class JobSummarySection
{
    /// <summary>
    /// Initializes a new instance of the JobSummarySection class.
    /// </summary>
    /// <param name="title">The title for the section (will be rendered as H2 header).</param>
    /// <param name="content">The content for the section. Can be plain text or markdown.</param>
    public JobSummarySection(string title, string content)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Gets the title for this section.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the content for this section. Can be plain text or markdown.
    /// </summary>
    public string Content { get; }
}