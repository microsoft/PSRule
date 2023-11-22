// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Management.Automation;
using Microsoft.CodeAnalysis.Sarif;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Pipeline.Dependencies;
using PSRule.Tool.Resources;
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
        private const int ERROR_MODUILE_FAILEDTOINSTALL = 501;

        private const int ERROR_MODULE_FAILEDTOFIND = 502;

        private const int ERROR_MODULE_ADD_VIOLATES_CONSTRAINT = 503;


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
            var file = LockFile.Read(null);
            var inputPath = operationOptions.InputPath == null || operationOptions.InputPath.Length == 0 ?
                new string[] { Environment.GetWorkingPath() } : operationOptions.InputPath;

            if (operationOptions.Path != null)
                option.Include.Path = operationOptions.Path;

            // Build command
            var builder = CommandLineBuilder.Assert(operationOptions.Module, option, host, file);
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
            var file = LockFile.Read(null);

            using var pwsh = PowerShell.Create();
            foreach (var kv in file.Modules)
            {
                var module = kv.Key;
                var targetVersion = kv.Value.Version;
                if (string.Equals(module, "PSRule", StringComparison.OrdinalIgnoreCase))
                    continue;

                invocation.Log(Messages.UsingModule, module, targetVersion.ToString());
                if (IsInstalled(pwsh, module, targetVersion, out var installedVersion) && !operationOptions.Force)
                {
                    if (operationOptions.Verbose)
                    {
                        invocation.Log($"The module {module} is already installed.");
                    }
                    continue;
                }

                var idealVersion = FindVersion(pwsh, module, null, targetVersion, null);
                if (idealVersion != null)
                    InstallVersion(invocation, pwsh, module, idealVersion.ToString());

                if (pwsh.HadErrors || (idealVersion == null && installedVersion == null))
                {
                    exitCode = ERROR_MODUILE_FAILEDTOINSTALL;
                    invocation.LogError(Messages.Error_501, module, targetVersion);
                    foreach (var error in pwsh.Streams.Error)
                    {
                        invocation.LogError(error.Exception.Message);
                    }
                }
            }
            return exitCode;
        }

        /// <summary>
        /// Add a module to the lock file.
        /// </summary>
        public static int AddModule(ModuleOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var option = GetOption(host);
            var requires = option.Requires.ToDictionary();
            var file = LockFile.Read(null);

            using var pwsh = PowerShell.Create();
            foreach (var module in operationOptions.Module)
            {
                if (!file.Modules.TryGetValue(module, out var item) || operationOptions.Force)
                {
                    // Get a constraint if set from options.
                    var moduleConstraint = requires.TryGetValue(module, out var c) ? c : null;

                    // Get target version if specified in command-line.
                    var targetVersion = !string.IsNullOrEmpty(operationOptions.Version) && SemanticVersion.TryParseVersion(operationOptions.Version, out var v) && v != null ? v : null;

                    // Check if the target version is valid with the constraint if set.
                    if (targetVersion != null && moduleConstraint != null && !moduleConstraint.Constraint.Equals(targetVersion))
                    {
                        invocation.LogError(Messages.Error_503, operationOptions.Version);
                        return ERROR_MODULE_ADD_VIOLATES_CONSTRAINT;
                    }

                    // Find the ideal version.
                    var idealVersion = FindVersion(pwsh, module, moduleConstraint, targetVersion, null);
                    if (idealVersion == null && targetVersion != null && operationOptions.SkipVerification)
                        idealVersion = targetVersion;

                    if (idealVersion == null)
                    {
                        invocation.LogError(Messages.Error_502, module);
                        return ERROR_MODULE_FAILEDTOFIND;
                    }

                    invocation.Log(Messages.UsingModule, module, idealVersion.ToString());
                    item = new LockEntry
                    {
                        Version = idealVersion
                    };
                    file.Modules[module] = item;
                }
                else
                {

                }
            }

            file.Write(null);
            return exitCode;
        }

        /// <summary>
        /// Remove a module from the lock file.
        /// </summary>
        public static int RemoveModule(ModuleOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var file = LockFile.Read(null);

            foreach (var module in operationOptions.Module)
            {
                if (file.Modules.TryGetValue(module, out var constraint))
                {
                    file.Modules.Remove(module);
                }
                else
                {

                }
            }

            file.Write(null);
            return exitCode;
        }

        /// <summary>
        /// Upgrade a module within the lock file.
        /// </summary>
        public static int UpgradeModule(ModuleOptions operationOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var exitCode = 0;
            var host = new ClientHost(invocation, operationOptions.Verbose, operationOptions.Debug);
            var option = GetOption(host);
            var requires = option.Requires.ToDictionary();
            var file = LockFile.Read(null);

            using var pwsh = PowerShell.Create();
            foreach (var kv in file.Modules)
            {
                // Get a constraint if set from options.
                var moduleConstraint = requires.TryGetValue(kv.Key, out var c) ? c : null;

                // Find the ideal version.
                var idealVersion = FindVersion(pwsh, kv.Key, moduleConstraint, null, null);
                if (idealVersion == null)
                {
                    invocation.LogError(Messages.Error_502, kv.Key);
                    return ERROR_MODULE_FAILEDTOFIND;
                }

                if (idealVersion == kv.Value.Version)
                    continue;

                invocation.Log(Messages.UsingModule, kv.Key, idealVersion.ToString());

                kv.Value.Version = idealVersion;
                file.Modules[kv.Key] = kv.Value;
            }

            file.Write(null);
            return exitCode;
        }

        private static bool IsInstalled(PowerShell pwsh, string module, SemanticVersion.Version targetVersion, out SemanticVersion.Version installedVersion)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Get-Module")
                .AddParameter(PARAM_NAME, module)
                .AddParameter("ListAvailable");

            var versions = pwsh.Invoke();
            installedVersion = null;
            foreach (var version in versions)
            {
                if (TryModuleInfo(version, out var versionString) &&
                    SemanticVersion.TryParseVersion(versionString, out var v) &&
                    (targetVersion == null || targetVersion.Equals(v)) &&
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

        private static SemanticVersion.Version FindVersion(PowerShell pwsh, string module, ModuleConstraint constraint, SemanticVersion.Version targetVersion, SemanticVersion.Version installedVersion)
        {
            pwsh.Commands.Clear();
            pwsh.Streams.ClearStreams();
            pwsh.AddCommand("Find-Module")
                .AddParameter(PARAM_NAME, module)
                .AddParameter("AllVersions");

            var versions = pwsh.Invoke();
            SemanticVersion.Version result = null;
            foreach (var version in versions)
            {
                if (version.Properties[PARAM_VERSION].Value is string versionString &&
                    SemanticVersion.TryParseVersion(versionString, out var v) &&
                    (constraint == null || constraint.Constraint.Equals(v)) &&
                    (targetVersion == null || targetVersion.Equals(v)) &&
                    v.CompareTo(result) > 0 &&
                    v.CompareTo(installedVersion) > 0)
                    result = v;
            }
            return result;
        }

        private static void InstallVersion(InvocationContext invocation, PowerShell pwsh, string name, string version)
        {
            invocation.Log(Messages.InstallingModule, name, version);

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
            option.Execution.InitialSessionState = Options.SessionState.Minimal;
            option.Input.Format = InputFormat.File;
            option.Output.Style ??= OutputStyle.Client;
            return option;
        }
    }
}
