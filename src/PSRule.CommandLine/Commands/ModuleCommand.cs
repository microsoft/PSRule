// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.CommandLine.Resources;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline.Dependencies;
using PSRule.CommandLine.Models;
using SemanticVersion = PSRule.Data.SemanticVersion;

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Execute features of the <c>module</c> command through the CLI.
/// </summary>
public sealed class ModuleCommand
{
    /// <summary>
    /// Failed to install a module.
    /// </summary>
    private const int ERROR_MODULE_FAILED_TO_INSTALL = 501;

    private const int ERROR_MODULE_FAILED_TO_FIND = 502;

    private const int ERROR_MODULE_ADD_VIOLATES_CONSTRAINT = 503;

    private const string PARAM_NAME = "Name";
    private const string PARAM_VERSION = "Version";

    private const string FIELD_PRERELEASE = "Prerelease";
    private const string FIELD_PSDATA = "PSData";
    private const string PRERELEASE_SEPARATOR = "-";

    /// <summary>
    /// Call <c>module restore</c>.
    /// </summary>
    public static int ModuleRestore(RestoreOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);

        using var pwsh = PowerShell.Create();

        // Restore from the lock file.
        foreach (var kv in file.Modules)
        {
            var module = kv.Key;
            var targetVersion = kv.Value.Version;
            if (string.Equals(module, "PSRule", StringComparison.OrdinalIgnoreCase))
                continue;

            // clientContext.LogVerbose(Messages.UsingModule, module, targetVersion.ToString());
            if (IsInstalled(pwsh, module, targetVersion, out var installedVersion) && !operationOptions.Force)
            {
                clientContext.LogVerbose($"The module {module} is already installed.");
                continue;
            }

            var idealVersion = FindVersion(pwsh, module, null, targetVersion, null);
            if (idealVersion != null)
                InstallVersion(clientContext, pwsh, module, idealVersion.ToString());

            if (pwsh.HadErrors || (idealVersion == null && installedVersion == null))
            {
                exitCode = ERROR_MODULE_FAILED_TO_INSTALL;
                clientContext.LogError(Messages.Error_501, module, targetVersion);
                foreach (var error in pwsh.Streams.Error)
                {
                    clientContext.LogError(error.Exception.Message);
                }
            }
        }

        // Restore from included modules.
        if (clientContext.Option?.Include?.Module != null && clientContext.Option.Include.Module.Length > 0)
        {
            foreach (var includeModule in clientContext.Option.Include.Module)
            {
                // Skip modules already in the lock unless force is used.
                if (file.Modules.TryGetValue(includeModule, out var lockEntry))
                    continue;

                // Get a constraint if set from options.
                var moduleConstraint = requires.TryGetValue(includeModule, out var c) ? c : null;

                // Check if the installed version matches the constraint.
                if (IsInstalled(pwsh, includeModule, null, out var installedVersion) &&
                    !operationOptions.Force &&
                    (moduleConstraint == null || moduleConstraint.Equals(installedVersion)))
                {
                    // invocation.Log(Messages.UsingModule, includeModule, installedVersion.ToString());
                    clientContext.LogVerbose($"The module {includeModule} is already installed.");
                    continue;
                }

                // Find the ideal version.
                var idealVersion = FindVersion(pwsh, includeModule, moduleConstraint, null, null);
                if (idealVersion != null)
                {
                    InstallVersion(clientContext, pwsh, includeModule, idealVersion.ToString());
                }
                else if (idealVersion == null)
                {
                    clientContext.LogError(Messages.Error_502, includeModule);
                    exitCode = ERROR_MODULE_FAILED_TO_FIND;
                }
                else if (pwsh.HadErrors)
                {
                    exitCode = ERROR_MODULE_FAILED_TO_INSTALL;
                    clientContext.LogError(Messages.Error_501, includeModule, idealVersion);
                    foreach (var error in pwsh.Streams.Error)
                    {
                        clientContext.LogError(error.Exception.Message);
                    }
                }
            }
        }

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    /// <summary>
    /// Initialize a new lock file based on existing options.
    /// </summary>
    public static int ModuleInit(ModuleOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = !operationOptions.Force ? LockFile.Read(null) : new LockFile();

        using var pwsh = PowerShell.Create();

        // Add for any included modules.
        if (clientContext.Option?.Include?.Module != null && clientContext.Option.Include.Module.Length > 0)
        {
            foreach (var includeModule in clientContext.Option.Include.Module)
            {
                // Skip modules already in the lock unless force is used.
                if (file.Modules.TryGetValue(includeModule, out var lockEntry))
                    continue;

                // Get a constraint if set from options.
                var moduleConstraint = requires.TryGetValue(includeModule, out var c) ? c : null;

                // Find the ideal version.
                var idealVersion = FindVersion(pwsh, includeModule, moduleConstraint, null, null);
                if (idealVersion == null)
                {
                    clientContext.LogError(Messages.Error_502, includeModule);
                    return ERROR_MODULE_FAILED_TO_FIND;
                }

                if (lockEntry?.Version == idealVersion)
                    continue;

                // invocation.Log(Messages.UsingModule, includeModule, idealVersion.ToString());

                file.Modules[includeModule] = new LockEntry
                {
                    Version = idealVersion
                };
            }
        }

        file.Write(null);

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    /// <summary>
    /// List any module and the installed versions from the lock file.
    /// </summary>
    public static int ModuleList(ModuleOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);

        var pwsh = PowerShell.Create();

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    /// <summary>
    /// Add a module to the lock file.
    /// </summary>
    public static int ModuleAdd(ModuleOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        if (operationOptions.Module == null || operationOptions.Module.Length == 0) return exitCode;

        var requires = clientContext.Option.Requires.ToDictionary();
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
                    clientContext.LogError(Messages.Error_503, operationOptions.Version!);
                    return ERROR_MODULE_ADD_VIOLATES_CONSTRAINT;
                }

                // Find the ideal version.
                var idealVersion = FindVersion(pwsh, module, moduleConstraint, targetVersion, null);
                if (idealVersion == null && targetVersion != null && operationOptions.SkipVerification)
                    idealVersion = targetVersion;

                if (idealVersion == null)
                {
                    clientContext.LogError(Messages.Error_502, module);
                    return ERROR_MODULE_FAILED_TO_FIND;
                }

                clientContext.LogVerbose(Messages.UsingModule, module, idealVersion.ToString());
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

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    /// <summary>
    /// Remove a module from the lock file.
    /// </summary>
    public static int ModuleRemove(ModuleOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        if (operationOptions.Module == null || operationOptions.Module.Length == 0) return exitCode;

        var file = LockFile.Read(null);

        using var pwsh = PowerShell.Create();
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

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    /// <summary>
    /// Upgrade a module within the lock file.
    /// </summary>
    public static int ModuleUpgrade(ModuleOptions operationOptions, ClientContext clientContext)
    {
        var exitCode = 0;
        var requires = clientContext.Option.Requires.ToDictionary();
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
                clientContext.LogError(Messages.Error_502, kv.Key);
                return ERROR_MODULE_FAILED_TO_FIND;
            }

            if (idealVersion == kv.Value.Version)
                continue;

            clientContext.LogVerbose(Messages.UsingModule, kv.Key, idealVersion.ToString());

            kv.Value.Version = idealVersion;
            file.Modules[kv.Key] = kv.Value;
        }

        file.Write(null);

        if (exitCode == 0)
        {
            ListModules(clientContext, GetModules(pwsh, file, clientContext.Option));
        }
        return exitCode;
    }

    #region Helper methods

    private static IEnumerable<ModuleRecord> GetModules(PowerShell pwsh, LockFile file, PSRuleOption? option)
    {
        var results = new List<ModuleRecord>();

        // Process modules in the lock file.
        foreach (var kv in file.Modules)
        {
            var installed = IsInstalled(pwsh, kv.Key, kv.Value.Version, out var installedVersion);
            results.Add(new ModuleRecord(
                Name: kv.Key,
                Version: kv.Value.Version.ToString(),
                Installed: installed,
                Locked: true
            ));
        }

        // Process included modules from options.
        if (option?.Include?.Module != null && option.Include.Module.Length > 0)
        {
            foreach (var includeModule in option.Include.Module)
            {
                // Skip modules already in the lock.
                if (file.Modules.ContainsKey(includeModule))
                    continue;

                var installed = IsInstalled(pwsh, includeModule, null, out var installedVersion);
                results.Add(new ModuleRecord(
                    Name: includeModule,
                    Version: installedVersion?.ToString() ?? "latest",
                    Installed: installed,
                    Locked: false
                ));
            }
        }

        return results;
    }

    private static void ListModules(ClientContext context, IEnumerable<ModuleRecord> results)
    {
        context.Invocation.Console.Out.Write(JsonConvert.SerializeObject(results, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }));
    }

    private static bool IsInstalled(PowerShell pwsh, string module, SemanticVersion.Version? targetVersion, [NotNullWhen(true)] out SemanticVersion.Version? installedVersion)
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
                versionString != null &&
                SemanticVersion.TryParseVersion(versionString, out var v) &&
                v != null &&
                (targetVersion == null || targetVersion.Equals(v)) &&
                v.CompareTo(installedVersion) > 0)
                installedVersion = v;
        }
        return installedVersion != null;
    }

    private static bool TryModuleInfo(PSObject value, out string? version)
    {
        version = null;
        if (value?.BaseObject is not PSModuleInfo info)
            return false;

        version = info.Version?.ToString();
        if (TryPrivateData(info, FIELD_PSDATA, out var psData) && psData != null && psData.ContainsKey(FIELD_PRERELEASE))
            version = string.Concat(version, PRERELEASE_SEPARATOR, psData[FIELD_PRERELEASE]?.ToString());

        return version != null;
    }

    private static bool TryPrivateData(PSModuleInfo info, string propertyName, out Hashtable? value)
    {
        value = null;
        if (info.PrivateData is Hashtable privateData && privateData.ContainsKey(propertyName) && privateData[propertyName] is Hashtable data)
        {
            value = data;
            return true;
        }
        return false;
    }

    private static SemanticVersion.Version? FindVersion(PowerShell pwsh, string module, ModuleConstraint? constraint, SemanticVersion.Version? targetVersion, SemanticVersion.Version? installedVersion)
    {
        pwsh.Commands.Clear();
        pwsh.Streams.ClearStreams();
        pwsh.AddCommand("Find-Module")
            .AddParameter(PARAM_NAME, module)
            .AddParameter("AllVersions");

        var versions = pwsh.Invoke();
        SemanticVersion.Version? result = null;
        foreach (var version in versions)
        {
            if (version.Properties[PARAM_VERSION].Value is string versionString &&
                SemanticVersion.TryParseVersion(versionString, out var v) &&
                v != null &&
                (constraint == null || constraint.Constraint.Equals(v)) &&
                (targetVersion == null || targetVersion.Equals(v)) &&
                v.CompareTo(result) > 0 &&
                v.CompareTo(installedVersion) > 0)
                result = v;
        }
        return result;
    }

    private static void InstallVersion([DisallowNull] ClientContext context, [DisallowNull] PowerShell pwsh, [DisallowNull] string name, [DisallowNull] string version)
    {
        context.LogVerbose(Messages.RestoringModule, name, version);

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

    #endregion Helper methods
}
