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
        WriteInput("Version", ClientBuilder.Version!);
    }

    /// <summary>
    /// Load in environment variables from the GitHub Action context.
    /// </summary>
    private string[] GetArgs(string[] args)
    {
        var result = new List<string>(args);

        if (Environment.TryString("INPUT_INCLUDEPATH", out var includePath) && !string.IsNullOrWhiteSpace(includePath))
        {
            result.Add("--path");
            result.Add(includePath);
            WriteInput("IncludePath", includePath);
        }

        if (Environment.TryString("INPUT_BASELINE", out var baseline) && !string.IsNullOrWhiteSpace(baseline))
        {
            result.Add("--baseline");
            result.Add(baseline);
            WriteInput("Baseline", baseline);
        }

        if (Environment.TryStringArray("INPUT_CONVENTIONS", [COMMA], out var conventions) && conventions != null)
        {
            foreach (var convention in conventions)
            {
                if (string.IsNullOrWhiteSpace(convention))
                    continue;

                result.Add("--convention");
                result.Add(convention);

                WriteInput("Convention", convention);
            }
        }

        if (Environment.TryString("INPUT_INPUTPATH", out var inputPath) && !string.IsNullOrWhiteSpace(inputPath))
        {
            result.Add("--input-path");
            result.Add(inputPath);
            WriteInput("InputPath", inputPath);
        }

        else if (Environment.TryStringArray("INPUT_FORMATS", [COMMA], out var formats) && formats != null)
        {
            foreach (var format in formats)
            {
                if (string.IsNullOrWhiteSpace(format))
                    continue;



                WriteInput("Format", format);
            }

            result.Add("--formats");
            result.Add(string.Join(" ", formats));
        }

        if (Environment.TryString("INPUT_OPTION", out var option) && !string.IsNullOrWhiteSpace(option))
        {
            result.Add("--option");
            result.Add(option);
            WriteInput("Option", option);
        }

        if (Environment.TryString("INPUT_OUTCOME", out var outcome) && !string.IsNullOrWhiteSpace(outcome))
        {
            result.Add("--outcome");
            result.Add(outcome);
            WriteInput("Outcome", outcome);
        }

        if (Environment.TryString("INPUT_OUTPUTFORMAT", out var outputFormat) && !string.IsNullOrWhiteSpace(outputFormat))
        {
            result.Add("--output");
            result.Add(outputFormat);
            WriteInput("OutputFormat", outputFormat);
        }

        if (Environment.TryString("INPUT_OUTPUTPATH", out var outputPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            result.Add("--output-path");
            result.Add(outputPath);
            WriteInput("OutputPath", outputPath);
        }

        if (Environment.TryString("INPUT_SUMMARY", out var summary) && !string.IsNullOrWhiteSpace(summary) && summary == "true" &&
            Environment.TryString("GITHUB_STEP_SUMMARY", out var jobSummaryPath) && !string.IsNullOrWhiteSpace(jobSummaryPath))
        {
            result.Add("--job-summary-path");
            result.Add(jobSummaryPath);
            WriteInput("Summary", summary);
        }

        if (Environment.TryStringArray("INPUT_MODULES", [COMMA], out var modules) && modules != null)
        {
            foreach (var module in modules)
            {
                if (string.IsNullOrWhiteSpace(module))
                    continue;

                var m = module.Trim();

                result.Add("--module");
                result.Add(m);

                WriteInput("Module", m);
            }
        }

        return [.. result];
    }

    private static void WriteInput(string name, string value)
    {
        Console.WriteLine($"[info] Using {name}: {value}");
    }
}
