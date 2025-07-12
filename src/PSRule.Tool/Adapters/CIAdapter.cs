// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Adapters;

/// <summary>
/// Base class for CI adapters.
/// Provides a common interface for CI-specific functionality.
/// </summary>
internal abstract class CIAdapter
{
    protected const char COMMA = ',';

    public string[] BuildArgs(string[] args)
    {
        WriteVersion();

        args = GetArgs(args);

        Console.WriteLine("");
        Console.WriteLine("---");

        return args;
    }

    protected abstract string[] GetArgs(string[] args);

    protected void WriteVersion()
    {
        WriteInput("Version", ClientBuilder.Version!);
    }

    protected static void WriteInput(string name, string value)
    {
        Console.WriteLine($"[info] Using {name}: {value}");
    }
}
