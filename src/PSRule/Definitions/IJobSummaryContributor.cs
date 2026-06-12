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
    /// A collection of job summary sections, where each section contains a title and content.
    /// The content can be formatted differently for different output formats (plain text for console, markdown for files).
    /// Returns null or empty collection if no additional content should be added.
    /// </returns>
    IEnumerable<JobSummarySection>? GetJobSummaryContent();
}
