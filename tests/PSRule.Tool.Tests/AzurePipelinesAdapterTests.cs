// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Tool.Adapters;

namespace PSRule.Tool;

public sealed class AzurePipelinesAdapterTests
{
    [Fact]
    public void BuildArgs_WithBasicInputs_ShouldReturnCorrectArgs()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };
        
        // Set up environment variables
        System.Environment.SetEnvironmentVariable("INPUT_MODULES", "PSRule.Rules.Azure");
        System.Environment.SetEnvironmentVariable("INPUT_INPUTPATH", ".");
        System.Environment.SetEnvironmentVariable("INPUT_OUTPUTFORMAT", "NUnit3");
        System.Environment.SetEnvironmentVariable("INPUT_OUTPUTPATH", "reports/");

        try
        {
            // Act
            var result = adapter.BuildArgs(args);

            // Assert
            Assert.Contains("run", result);
            Assert.Contains("--module", result);
            Assert.Contains("PSRule.Rules.Azure", result);
            Assert.Contains("--input-path", result);
            Assert.Contains(".", result);
            Assert.Contains("--output", result);
            Assert.Contains("NUnit3", result);
            Assert.Contains("--output-path", result);
            Assert.Contains("reports/", result);
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INPUT_MODULES", null);
            System.Environment.SetEnvironmentVariable("INPUT_INPUTPATH", null);
            System.Environment.SetEnvironmentVariable("INPUT_OUTPUTFORMAT", null);
            System.Environment.SetEnvironmentVariable("INPUT_OUTPUTPATH", null);
        }
    }

    [Fact]
    public void BuildArgs_WithMultipleModules_ShouldReturnCorrectArgs()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };
        
        // Set up environment variables
        System.Environment.SetEnvironmentVariable("INPUT_MODULES", "PSRule.Rules.Azure,PSRule.Rules.Kubernetes");

        try
        {
            // Act
            var result = adapter.BuildArgs(args);

            // Assert
            Assert.Contains("run", result);
            
            // Find all --module instances
            var moduleIndexes = result
                .Select((value, index) => new { value, index })
                .Where(x => x.value == "--module")
                .Select(x => x.index)
                .ToList();

            Assert.Equal(2, moduleIndexes.Count);
            Assert.Contains("PSRule.Rules.Azure", result);
            Assert.Contains("PSRule.Rules.Kubernetes", result);
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INPUT_MODULES", null);
        }
    }

    [Fact]
    public void BuildArgs_WithSummaryEnabled_ShouldAddJobSummaryPath()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };
        
        // Set up environment variables
        System.Environment.SetEnvironmentVariable("INPUT_SUMMARY", "true");

        try
        {
            // Act
            var result = adapter.BuildArgs(args);

            // Assert
            Assert.Contains("run", result);
            Assert.Contains("--job-summary-path", result);
            Assert.Contains("reports/summary.md", result);
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INPUT_SUMMARY", null);
        }
    }

    [Fact]
    public void BuildArgs_WithConventions_ShouldReturnCorrectArgs()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };
        
        // Set up environment variables
        System.Environment.SetEnvironmentVariable("INPUT_CONVENTIONS", "Azure.Default,Azure.GA_2023_09");

        try
        {
            // Act
            var result = adapter.BuildArgs(args);

            // Assert
            Assert.Contains("run", result);
            
            // Find all --convention instances
            var conventionIndexes = result
                .Select((value, index) => new { value, index })
                .Where(x => x.value == "--convention")
                .Select(x => x.index)
                .ToList();

            Assert.Equal(2, conventionIndexes.Count);
            Assert.Contains("Azure.Default", result);
            Assert.Contains("Azure.GA_2023_09", result);
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INPUT_CONVENTIONS", null);
        }
    }

    [Fact]
    public void BuildArgs_WithBaseline_ShouldReturnCorrectArgs()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };
        
        // Set up environment variables
        System.Environment.SetEnvironmentVariable("INPUT_BASELINE", "Azure.Default");

        try
        {
            // Act
            var result = adapter.BuildArgs(args);

            // Assert
            Assert.Contains("run", result);
            Assert.Contains("--baseline", result);
            Assert.Contains("Azure.Default", result);
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INPUT_BASELINE", null);
        }
    }

    [Fact]
    public void BuildArgs_WithEmptyInputs_ShouldReturnOriginalArgs()
    {
        // Arrange
        var adapter = new AzurePipelinesAdapter();
        var args = new string[] { "run" };

        // Act
        var result = adapter.BuildArgs(args);

        // Assert
        Assert.Contains("run", result);
        // Should only contain the original argument
        Assert.Single(result.Where(x => x != "run"), x => x == "run" || x.StartsWith("["));
    }
}