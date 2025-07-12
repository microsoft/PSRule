// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace PSRule.Tool.Adapters;

/// <summary>
/// Choose an appropriate adapter based on command line arguments.
/// This is used to adapt the command line arguments for different CI/CD environments.
/// </summary>
internal sealed class AdapterBuilder
{
    public static bool TryAdapter(string[] args, [NotNullWhen(true)] out Func<string[], Task<int>>? execute)
    {
        execute = null;
        if (ShouldUseGitHubActionAdapter(args))
        {
            var adapter = new GitHubActionsAdapter();
            args = adapter.BuildArgs(args);

            execute = async (string[] args) =>
            {
                return await ClientBuilder.New().InvokeAsync(args);
            };
        }
        else if (ShouldUseAzurePipelinesAdapter(args))
        {
            var adapter = new AzurePipelinesAdapter();
            args = adapter.BuildArgs(args);

            execute = async (string[] args) =>
            {
                return await ClientBuilder.New().InvokeAsync(args);
            };
        }

        return execute != null;
    }

    private static bool ShouldUseGitHubActionAdapter(string[] args)
    {
        if (args == null || args.Length == 0)
            return false;

        foreach (var arg in args)
        {
            if (arg.Equals("--in-github-actions", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool ShouldUseAzurePipelinesAdapter(string[] args)
    {
        if (args == null || args.Length == 0)
            return false;

        foreach (var arg in args)
        {
            if (arg.Equals("--in-azure-pipelines", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
