// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Commands;

public abstract class BaseTests
{
    protected static string GetSourcePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
    }

    protected static ClientContext ClientContext(string? option = null, IConsole? console = null, string? workingPath = null)
    {
        return new ClientContext
        (
            console: console ?? new Console(),
            option: option,
            verbose: false,
            debug: false,
            workingPath: workingPath
        );
    }
}
