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

        // For now, return a simple message to test the command structure
        var result = new
        {
            message = "Get rule command is working!",
            options = new
            {
                workingPath = workingPath,
                operationOptions.Path,
                operationOptions.Module,
                operationOptions.Name,
                operationOptions.Baseline,
                operationOptions.IncludeDependencies,
                operationOptions.NoRestore
            }
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        clientContext.Host.WriteHost(json);

        return Task.FromResult(0);
    }
}