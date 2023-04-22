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
        private const string PUBLISHER = "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US";

        /// <summary>
        /// A generic error.
        /// </summary>
        private const int ERROR_GENERIC = 1;

        /// <summary>
        /// Failed to install a module.
        /// </summary>
        private const int ERROR_MODUILE_FAILEDTOINSTALL = 500;

        /// <summary>
        /// One or more failures occurred.
        /// </summary>
        private const int ERROR_BREAK_ON_FAILURE = 100;

        public static int RunAnalyze(AnalyzerOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var option = GetOption(host);
            var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
                new string[] { PSRuleOption.GetWorkingPath() } : operationOptions.InputPath;

            if (operationOptions.Path != null)
                option.Include.Path = operationOptions.Path;

            // Build command
            var builder = CommandLineBuilder.Assert(operationOptions.Module, option, host);
            builder.Baseline(BaselineOption.FromString(operationOptions.Baseline));
            builder.InputPath(inputPath);
            builder.UnblockPublisher(PUBLISHER);

            using var pipeline = builder.Build();
            if (pipeline != null)
            {
                pipeline.Begin();
                pipeline.Process(null);
                pipeline.End();
                if (pipeline.Result.HadFailures)
                    exitCode = ERROR_BREAK_ON_FAILURE;
            }
            return host.HadErrors || pipeline == null ? ERROR_GENERIC : exitCode;
        }

        public static int RunRestore(RestoreOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var option = GetOption(host);
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
                    exitCode = ERROR_MODUILE_FAILEDTOINSTALL;
                    invocation.Console.Error.Write($"Failed to install {requires[i].Module}.");
                    foreach (var error in pwsh.Streams.Error)
                    {
                        invocation.Console.Error.Write(error.Exception.Message);
                    }
                }
            }
            return exitCode;
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

        private static PSRuleOption GetOption(ClientHost host)
        {
            PSRuleOption.UseHostContext(host);
            var option = PSRuleOption.FromFileOrEmpty();
            option.Execution.InitialSessionState = Configuration.SessionState.Minimal;
            option.Input.Format = InputFormat.File;
            option.Output.Style = OutputStyle.Client;
            return option;
        }
    }
}
