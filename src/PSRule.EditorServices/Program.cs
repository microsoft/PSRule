// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.EditorServices;

static class Program
{
    /// <summary>
    /// Entry point for CLI tool.
    /// </summary>
    static async Task<int> Main(string[] args)
    {
        var modulePath = Environment.CombineEnvironmentVariable(
            ModuleIntrinsics.GetPSModulePath(ModuleIntrinsics.PSModulePathScope.User),
            Path.Combine(Environment.GetRootedBasePath(AppContext.BaseDirectory), "Modules")
       );

        System.Environment.SetEnvironmentVariable("PSModulePath", modulePath, EnvironmentVariableTarget.Process);
        return await ClientBuilder.New().Parse(args).InvokeAsync();
    }
}
