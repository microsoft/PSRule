// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using SemanticVersion = PSRule.Data.SemanticVersion;

namespace PSRule.Tool
{
    internal sealed class ClientHelper
    {
        public static void RunAnalyze(AnalyzerOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var option = GetOption();
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
                new string[] { PSRuleOption.GetWorkingPath() } : operationOptions.InputPath;

            // Build command
            var builder = CommandLineBuilder.Assert(operationOptions.Module, option, host);
            builder.Baseline(BaselineOption.FromString(operationOptions.Baseline));
            builder.InputPath(inputPath);

            using var pipeline = builder.Build();
            if (pipeline != null)
            {
                pipeline.Begin();
                pipeline.Process(null);
                pipeline.End();
            }
        }

        public static void RunRestore(RestoreOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var option = GetOption();
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var requires = option.Requires.ToArray();

            using var pwsh = PowerShell.Create();
            for (var i = 0; i < requires.Length; i++)
            {
                invocation.Console.WriteLine($"Getting {requires[i].Module}.");
                var version = GetVersion(pwsh, requires[i]);

                invocation.Console.WriteLine($"Installing {requires[i].Module} v{version}.");
                InstallVersion(pwsh, requires[i].Module, version);

                if (pwsh.HadErrors)
                {
                    invocation.Console.Error.Write($"Failed to install {requires[i].Module}.");
                    foreach (var error in pwsh.Streams.Error)
                    {
                        invocation.Console.Error.Write(error.Exception.Message);
                    }
                }
            }
        }

        private static string GetVersion(PowerShell pwsh, ModuleConstraint constraint)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Find-Module")
                .AddParameter("Name", constraint.Module)
                .AddParameter("AllVersions");

            var versions = pwsh.Invoke();
            SemanticVersion.Version result = null;
            foreach (var version in versions)
            {
                if (version.Properties["Version"].Value is string versionString &&
                    SemanticVersion.TryParseVersion(versionString, out var v) &&
                    constraint.Constraint.Equals(v) &&
                    v.CompareTo(result) > 0)
                    result = v;
            }
            return result?.ToString();
        }

        private static void InstallVersion(PowerShell pwsh, string name, string version)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Install-Module")
                    .AddParameter("Name", name)
                    .AddParameter("Scope", "CurrentUser")
                    .AddParameter("Force");

            pwsh.Invoke();
        }

        private static PSRuleOption GetOption()
        {
            var option = PSRuleOption.FromFileOrEmpty();
            option.Execution.InitialSessionState = Configuration.SessionState.Minimal;
            option.Input.Format = InputFormat.File;
            option.Output.Style = OutputStyle.Client;
            return option;
        }
    }
}
