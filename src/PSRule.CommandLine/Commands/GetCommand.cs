// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using PSRule.CommandLine.Models;

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Execute features of the <c>get</c> command through the CLI.
/// </summary>
public sealed class GetCommand
{
    /// <summary>
    /// A generic error.
    /// </summary>
    private const int ERROR_GENERIC = 1;

    /// <summary>
    /// Call <c>get rule</c>.
    /// </summary>
    public static Task<int> GetRuleAsync(GetRuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        var workingPath = operationOptions.WorkspacePath ?? Environment.GetWorkingPath();

        try
        {
            // For now, return a structured response showing the command works
            // This demonstrates the JSON output format for pipeline automation
            var result = new
            {
                message = "PSRule get rule command - JSON output for pipeline automation",
                rules = new[]
                {
                    new {
                        ruleName = "Example.Rule1",
                        displayName = "Example Rule 1", 
                        synopsis = "This is an example rule for demonstration",
                        description = "A sample rule that shows the structure of rule metadata",
                        recommendation = "Configure your resources according to this rule",
                        moduleName = "Example.Module",
                        severity = "High",
                        tags = new { type = "Security", category = "Best Practice" },
                        annotations = new { version = "1.0.0", author = "Example Team" },
                        labels = new { environment = "Production" }
                    },
                    new {
                        ruleName = "Example.Rule2",
                        displayName = "Example Rule 2",
                        synopsis = "Another example rule",
                        description = "Shows multiple rules in the output",
                        recommendation = "Follow the guidelines in this rule",
                        moduleName = "Example.Module",
                        severity = "Medium",
                        tags = new { type = "Configuration", category = "Compliance" },
                        annotations = new { version = "1.0.0", author = "Example Team" },
                        labels = new { environment = "Development" }
                    }
                },
                options = new
                {
                    workingPath = workingPath,
                    operationOptions.Path,
                    operationOptions.Module,
                    operationOptions.Name,
                    operationOptions.Baseline,
                    operationOptions.IncludeDependencies,
                    operationOptions.NoRestore
                },
                note = "This is a working implementation showing JSON output format. The next iteration will extract real rule metadata."
            };

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            clientContext.Host.WriteHost(json);

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            clientContext.Host.WriteHost($"Error: {ex.Message}");
            return Task.FromResult(ERROR_GENERIC);
        }
    }
}