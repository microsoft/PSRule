// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Definitions;

namespace PSRule.Pipeline.Output;

#nullable enable

/// <summary>
/// Test implementation of IJobSummaryContributor for testing purposes.
/// </summary>
internal sealed class TestJobSummaryContributor(string title, string content) : IJobSummaryContributor
{
    private readonly string _Title = title;
    private readonly string _Content = content;

    public IEnumerable<JobSummarySection>? GetJobSummaryContent()
    {
        if (string.IsNullOrWhiteSpace(_Title) || string.IsNullOrWhiteSpace(_Content))
            return null;

        return [new JobSummarySection(_Title, new InfoString(_Content))];
    }
}

#nullable restore
