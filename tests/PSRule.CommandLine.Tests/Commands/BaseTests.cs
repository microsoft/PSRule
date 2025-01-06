// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace PSRule.CommandLine.Commands;

public abstract class BaseTests
{
    protected static string GetSourcePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
    }

    protected static ClientContext ClientContext(InvocationContext? invocationContext = null, string? option = null, IConsole? console = null, string? workingPath = null)
    {
        return new ClientContext
        (
            invocation: invocationContext ?? InvocationContext(console),
            option: option,
            verbose: false,
            debug: false,
            workingPath: workingPath
        );
    }

    protected static InvocationContext InvocationContext(IConsole? console = null)
    {
        var p = new Parser();
        var result = p.Parse(string.Empty);
        return new InvocationContext(result, console);
    }
}
