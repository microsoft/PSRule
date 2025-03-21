// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Adapters;

/// <summary>
/// This is an adapter for handling GitHub Actions specific functionality
/// for the official PSRule GitHub Action.
/// </summary>
internal sealed class GitHubActionsAdapter
{
    private const char COMMA = ',';

    public string[] BuildArgs(string[] args)
    {
        WriteVersion();

        args = GetArgs(args);

        Console.WriteLine("");
        Console.WriteLine("---");

        return args;
    }

    private void WriteVersion()
    {
        Console.WriteLine($"[info] Using Version: {ClientBuilder.Version}");
        // Console.WriteLine($"[info] GitHub Action Version: {Environment.GetGitHubActionVersion()}");
        // Console.WriteLine($"[info] GitHub Action Name: {Environment.GetGitHubActionName()}");
        // Console.WriteLine($"[info] GitHub Action ID: {Environment.GetGitHubActionId()}");
        // Console.WriteLine($"[info] GitHub Action Path: {Environment.GetGitHubActionPath()}");
    }

    private string[] GetArgs(string[] args)
    {
        var result = new List<string>(args);

        if (Environment.TryString("INPUT_INCLUDEPATH", out var includePath) && !string.IsNullOrWhiteSpace(includePath))
        {
            result.Add("--path");
            result.Add(includePath);
            Console.WriteLine($"[info] Using IncludePath: {includePath}");
        }

        if (Environment.TryString("INPUT_BASELINE", out var baseline) && !string.IsNullOrWhiteSpace(baseline))
        {
            result.Add("--baseline");
            result.Add(baseline);
            Console.WriteLine($"[info] Using Baseline: {baseline}");
        }

        // CLI does not support this yet.
        // if (Environment.TryString("INPUT_CONVENTIONS", out var conventions) && !string.IsNullOrWhiteSpace(conventions))
        // {
        //     foreach (var convention in conventions.Split([COMMA], StringSplitOptions.RemoveEmptyEntries))
        //     {
        //         if (string.IsNullOrWhiteSpace(convention))
        //             continue;

        //         result.Add("--convention");
        //         result.Add(convention.Trim());
        //     }
        //     Console.WriteLine($"[info] Using Conventions: {conventions}");
        // }

        if (Environment.TryString("INPUT_INPUTPATH", out var inputPath) && !string.IsNullOrWhiteSpace(inputPath))
        {
            result.Add("--input-path");
            result.Add(inputPath);
            Console.WriteLine($"[info] Using InputPath: {inputPath}");
        }

        if (Environment.TryString("INPUT_OPTION", out var option) && !string.IsNullOrWhiteSpace(option))
        {
            result.Add("--option");
            result.Add(option);
            Console.WriteLine($"[info] Using Option: {option}");
        }

        if (Environment.TryString("INPUT_OUTCOME", out var outcome) && !string.IsNullOrWhiteSpace(outcome))
        {
            result.Add("--outcome");
            result.Add(outcome);
            Console.WriteLine($"[info] Using Outcome: {outcome}");
        }

        if (Environment.TryString("INPUT_OUTPUTFORMAT", out var outputFormat) && !string.IsNullOrWhiteSpace(outputFormat))
        {
            result.Add("--output-format");
            result.Add(outputFormat);
            Console.WriteLine($"[info] Using OutputFormat: {outputFormat}");
        }

        if (Environment.TryString("INPUT_OUTPUTPATH", out var outputPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            result.Add("--output-path");
            result.Add(outputPath);
            Console.WriteLine($"[info] Using OutputPath: {outputPath}");
        }

        if (Environment.TryString("INPUT_SUMMARY", out var summary) && !string.IsNullOrWhiteSpace(summary) && summary == "true" &&
            Environment.TryString("GITHUB_STEP_SUMMARY", out var jobSummaryPath) && !string.IsNullOrWhiteSpace(jobSummaryPath))
        {
            result.Add("--job-summary-path");
            result.Add(jobSummaryPath);
            Console.WriteLine($"[info] Using Summary: {jobSummaryPath}");
        }

        if (Environment.TryString("INPUT_MODULES", out var modules) && !string.IsNullOrWhiteSpace(modules))
        {
            foreach (var module in modules.Split([COMMA], StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(module))
                    continue;

                result.Add("--module");
                result.Add(module.Trim());
            }
            Console.WriteLine($"[info] Using Modules: {modules}");
        }

        return [.. result];
    }
}
