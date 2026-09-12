# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Test convention that demonstrates job summary contribution capabilities
# Note: To actually contribute to job summaries, a convention would need to be implemented in C#
# and inherit from BaseConvention while implementing IJobSummaryContributor

Convention 'Test.JobSummaryConvention' {
    Begin {
        Write-Host "Test convention for job summary contribution"
    }
}
