// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Management.Automation;

namespace PSRule.Tool;

static class Program
{
    /// <summary>
    /// Entry point for CLI tool.
    /// </summary>
    static async Task<int> Main(string[] args)
    {
        var ps = ModuleIntrinsics.GetPSModulePath(ModuleIntrinsics.PSModulePathScope.User);
        System.Environment.SetEnvironmentVariable("PSModulePath", ps, EnvironmentVariableTarget.Process);

        return await ClientBuilder.New().InvokeAsync(args);
    }
}
