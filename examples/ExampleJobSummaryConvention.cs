// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions;
using PSRule.Definitions.Conventions;
using PSRule.Runtime;

namespace PSRule.Examples;

/// <summary>
/// Example convention that demonstrates how to contribute content to job summaries.
/// This shows how convention authors can implement IJobSummaryContributor to extend job summary output.
/// </summary>
internal sealed class ExampleJobSummaryConvention : BaseConvention, IConventionV1, IJobSummaryContributor
{
    private int _processedCount = 0;
    private int _successCount = 0;

    public ExampleJobSummaryConvention(ISourceFile source, string name) : base(source, name)
    {
    }

    public override void Process(LegacyRunspaceContext context, IEnumerable input)
    {
        // Example: Track some metrics during processing
        foreach (var item in input)
        {
            _processedCount++;
            // Simulate some processing logic
            if (_processedCount % 2 == 0)
                _successCount++;
        }
    }

    public IEnumerable<JobSummarySection>? GetJobSummaryContent()
    {
        // Return custom sections for the job summary
        var sections = new List<JobSummarySection>();

        // Add a metrics section
        var metricsContent = $@"The example convention processed {_processedCount} items with {_successCount} successful operations.

### Breakdown
- Total items: {_processedCount}
- Successful: {_successCount}
- Success rate: {(_processedCount > 0 ? (_successCount * 100.0 / _processedCount):0):F1}%";

        sections.Add(new JobSummarySection("Convention Metrics", new InfoString(metricsContent)));

        // Add an additional information section
        var additionalInfo = @"This section demonstrates how conventions can contribute custom content to job summaries.

Convention authors can:
- Add custom metrics and statistics
- Provide configuration summaries
- Include environment information
- Display custom analysis results

For more information, see the [PSRule conventions documentation](https://microsoft.github.io/PSRule/concepts/conventions/).";

        sections.Add(new JobSummarySection("Convention Information", new InfoString(additionalInfo)));

        return sections;
    }

    #region IConventionV1 implementation (required but not relevant for this example)
    
    public IResourceHelpInfo Info => throw new NotImplementedException();
    public ResourceFlags Flags => ResourceFlags.None;
    public ISourceExtent Extent => throw new NotImplementedException();
    public ResourceKind Kind => ResourceKind.Convention;
    public string ApiVersion => "v1";
    public ResourceId? Ref => null;
    public ResourceId[]? Alias => null;
    public IResourceTags? Tags => null;
    public IResourceLabels? Labels => null;

    #endregion
}