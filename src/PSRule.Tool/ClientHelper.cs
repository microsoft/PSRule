// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
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

        private const string PARAM_NAME = "Name";
        private const string PARAM_VERSION = "Version";

        private const string FIELD_PRERELEASE = "Prerelease";
        private const string FIELD_PSDATA = "PSData";
        private const string PRERELEASE_SEPARATOR = "-";

        public static int RunAnalyze(AnalyzerOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var option = GetOption(host);
            var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
                new string[] { Environment.GetWorkingPath() } : operationOptions.InputPath;

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
                if (string.Equals(requires[i].Module, "PSRule", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                invocation.Console.WriteLine($"Getting {requires[i].Module}.");
                if (IsInstalled(pwsh, requires[i], out var installedVersion) && !operationOptions.Force)
                    continue;

                var idealVersion = FindVersion(pwsh, requires[i], installedVersion);
                if (idealVersion != null)
                {
                    var version = idealVersion.ToString();
                    invocation.Console.WriteLine($"Installing {requires[i].Module} v{version}.");
                    InstallVersion(pwsh, requires[i].Module, version);
                }

                if (pwsh.HadErrors || (idealVersion == null && installedVersion == null))
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

        private static bool IsInstalled(PowerShell pwsh, ModuleConstraint constraint, out SemanticVersion.Version installedVersion)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Get-Module")
                .AddParameter(PARAM_NAME, constraint.Module)
                .AddParameter("ListAvailable");

            var versions = pwsh.Invoke();
            installedVersion = null;
            foreach (var version in versions)
            {
                if (TryModuleInfo(version, out var versionString) &&
                    SemanticVersion.TryParseVersion(versionString, out var v) &&
                    constraint.Constraint.Equals(v) &&
                    v.CompareTo(installedVersion) > 0)
                    installedVersion = v;
            }
            return installedVersion != null;
        }

        private static bool TryModuleInfo(PSObject value, out string version)
        {
            version = null;
            if (value?.BaseObject is not PSModuleInfo info)
                return false;

            version = info.Version?.ToString();
            if (TryPrivateData(info, FIELD_PSDATA, out var psData) && psData.ContainsKey(FIELD_PRERELEASE))
                version = string.Concat(version, PRERELEASE_SEPARATOR, psData[FIELD_PRERELEASE].ToString());

            return version != null;
        }

        private static bool TryPrivateData(PSModuleInfo info, string propertyName, out Hashtable value)
        {
            value = null;
            if (info.PrivateData is Hashtable privateData && privateData.ContainsKey(propertyName) && privateData[propertyName] is Hashtable data)
            {
                value = data;
                return true;
            }
            return false;
        }

        private static SemanticVersion.Version FindVersion(PowerShell pwsh, ModuleConstraint constraint, SemanticVersion.Version installedVersion)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Find-Module")
                .AddParameter(PARAM_NAME, constraint.Module)
                .AddParameter("AllVersions");

            var versions = pwsh.Invoke();
            SemanticVersion.Version result = null;
            foreach (var version in versions)
            {
                if (version.Properties[PARAM_VERSION].Value is string versionString &&
                    SemanticVersion.TryParseVersion(versionString, out var v) &&
                    constraint.Constraint.Equals(v) &&
                    v.CompareTo(result) > 0 &&
                    v.CompareTo(installedVersion) > 0)
                    result = v;
            }
            return result;
        }

        private static void InstallVersion(PowerShell pwsh, string name, string version)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Install-Module")
                    .AddParameter(PARAM_NAME, name)
                    .AddParameter("RequiredVersion", version)
                    .AddParameter("Scope", "CurrentUser")
                    .AddParameter("AllowPrerelease")
                    .AddParameter("Force");

            pwsh.Invoke();
        }

        private static PSRuleOption GetOption(ClientHost host)
        {
            PSRuleOption.UseHostContext(host);
            var option = PSRuleOption.FromFileOrEmpty();
            option.Execution.InitialSessionState = Configuration.SessionState.Minimal;
            option.Input.Format = InputFormat.File;
            option.Output.Style ??= OutputStyle.Client;
            return option;
        }
    }
}
