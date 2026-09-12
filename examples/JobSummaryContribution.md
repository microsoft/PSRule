# PSRule Job Summary Convention Extension Example

This example demonstrates how conventions can contribute additional information to PSRule job summaries.

## Overview

The new `IJobSummaryContributor` interface allows conventions to append custom sections to job summary output. This enables:

- Custom metrics and statistics
- Environment information
- Configuration summaries  
- Additional analysis results

## Usage

Conventions can implement the `IJobSummaryContributor` interface to contribute content:

```csharp
public class MyConvention : BaseConvention, IConventionV1, IJobSummaryContributor
{
    public IEnumerable<JobSummarySection>? GetJobSummaryContent()
    {
        return new[] 
        {
            new JobSummarySection("Custom Metrics", "- Processed: 100 items\n- Success rate: 95%"),
            new JobSummarySection("Environment", "- OS: Windows\n- Runtime: .NET 8.0")
        };
    }
    
    // ... other convention implementation
}
```

## Example Output

The job summary will include additional sections after the standard PSRule content:

```markdown
# PSRule result summary

❌ PSRule completed with an overall result of 'Fail' with 10 rule(s) and 5 target(s) in 00:00:02.123.

## Analysis
...

## Custom Metrics
- Processed: 100 items
- Success rate: 95%

## Environment  
- OS: Windows
- Runtime: .NET 8.0
```

## Backward Compatibility

This feature is fully backward compatible. Existing conventions and job summaries will continue to work unchanged. Only conventions that explicitly implement `IJobSummaryContributor` will contribute additional content.