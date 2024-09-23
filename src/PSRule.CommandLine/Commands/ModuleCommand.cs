// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using PSRule.CommandLine.Models;
using PSRule.CommandLine.Resources;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline.Dependencies;
using SemanticVersion = PSRule.Data.SemanticVersion;
using NuGet.Packaging;
using NuGet.Common;
using PSRule.Pipeline;

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
    private const string FIELD_PRERELEASE = "Prerelease";
    private const string FIELD_PSDATA = "PSData";
    private const string PRERELEASE_SEPARATOR = "-";
    private const string POWERSHELL_GALLERY_SOURCE = "https://www.powershellgallery.com/api/v2/";
    private const string MODULES_PATH = "Modules";

    /// <summary>
    /// Call <c>module restore</c>.
    /// </summary>
    public static async Task<int> ModuleRestoreAsync(RestoreOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
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

            var idealVersion = await FindVersionAsync(module, null, targetVersion, null, cancellationToken);
            if (idealVersion != null)
                await InstallVersionAsync(clientContext, module, idealVersion.ToString(), cancellationToken);

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
                    (moduleConstraint == null || moduleConstraint.Accepts(installedVersion)))
                {
                    // invocation.Log(Messages.UsingModule, includeModule, installedVersion.ToString());
                    clientContext.LogVerbose($"The module {includeModule} is already installed.");
                    continue;
                }

                // Find the ideal version.
                var idealVersion = await FindVersionAsync(includeModule, moduleConstraint, null, null, cancellationToken);
                if (idealVersion != null)
                {
                    await InstallVersionAsync(clientContext, includeModule, idealVersion.ToString(), cancellationToken);
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
    public static async Task<int> ModuleInitAsync(ModuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
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
                var idealVersion = await FindVersionAsync(includeModule, moduleConstraint, null, null, cancellationToken);
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task<int> ModuleListAsync(ModuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
    public static async Task<int> ModuleAddAsync(ModuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
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
                var moduleConstraint = requires.TryGetValue(module, out var c) ? c : ModuleConstraint.Any(module, includePrerelease: false);

                // Get target version if specified in command-line.
                var targetVersion = !string.IsNullOrEmpty(operationOptions.Version) && SemanticVersion.TryParseVersion(operationOptions.Version, out var v) && v != null ? v : null;

                // Check if the target version is valid with the constraint if set.
                if (targetVersion != null && moduleConstraint != null && !moduleConstraint.Constraint.Accepts(targetVersion))
                {
                    clientContext.LogError(Messages.Error_503, operationOptions.Version!);
                    return ERROR_MODULE_ADD_VIOLATES_CONSTRAINT;
                }

                // Find the ideal version.
                var idealVersion = await FindVersionAsync(module, moduleConstraint, targetVersion, null, cancellationToken);
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task<int> ModuleRemoveAsync(ModuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
    public static async Task<int> ModuleUpgradeAsync(ModuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        var exitCode = 0;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);

        using var pwsh = PowerShell.Create();
        foreach (var kv in file.Modules)
        {
            // Get a constraint if set from options.
            var moduleConstraint = requires.TryGetValue(kv.Key, out var c) ? c : ModuleConstraint.Any(kv.Key, includePrerelease: false);

            // Find the ideal version.
            var idealVersion = await FindVersionAsync(kv.Key, moduleConstraint, null, null, cancellationToken);
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
                (targetVersion == null || targetVersion.CompareTo(v) == 0) &&
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

    private static async Task<SemanticVersion.Version?> FindVersionAsync(string module, ModuleConstraint? constraint, SemanticVersion.Version? targetVersion, SemanticVersion.Version? installedVersion, CancellationToken cancellationToken)
    {
        var cache = new SourceCacheContext();
        var logger = new NullLogger();
        var resource = await GetSourceRepositoryAsync();
        var versions = await resource.GetAllVersionsAsync(module, cache, logger, cancellationToken);

        SemanticVersion.Version? result = null;
        foreach (var version in versions)
        {
            if (version.ToFullString() is string versionString &&
                SemanticVersion.TryParseVersion(versionString, out var v) &&
                v != null &&
                (constraint == null || constraint.Accepts(v)) &&
                (targetVersion == null || targetVersion.CompareTo(v) == 0) &&
                v.CompareTo(result) > 0 &&
                v.CompareTo(installedVersion) > 0)
                result = v;
        }
        return result;
    }

    private static async Task InstallVersionAsync([DisallowNull] ClientContext context, [DisallowNull] string name, [DisallowNull] string version, CancellationToken cancellationToken)
    {
        context.LogVerbose(Messages.RestoringModule, name, version);

        var cache = new SourceCacheContext();
        var logger = new NullLogger();
        var resource = await GetSourceRepositoryAsync();

        var packageVersion = new NuGetVersion(version);
        using var packageStream = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(
            name,
            packageVersion,
            packageStream,
            cache,
            logger,
            cancellationToken);

        using var packageReader = new PackageArchiveReader(packageStream);
        var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

        var modulePath = GetModulePath(context, name, version);

        // Remove existing module.
        if (Directory.Exists(modulePath))
            Directory.Delete(modulePath, true);

        var files = packageReader.GetFiles();
        packageReader.CopyFiles(modulePath, files, (name, targetPath, s) =>
        {
            if (ShouldIgnorePackageFile(name))
                return null;

            s.CopyToFile(targetPath);

            return targetPath;

        }, logger, cancellationToken);
    }

    private static async Task<FindPackageByIdResource> GetSourceRepositoryAsync()
    {
        var source = new PackageSource(POWERSHELL_GALLERY_SOURCE);
        var repository = Repository.Factory.GetCoreV2(source);
        return await repository.GetResourceAsync<FindPackageByIdResource>();
    }

    private static string GetModulePath(ClientContext context, string name, string version)
    {
        return Path.Combine(context.CachePath, MODULES_PATH, name, version);
    }

    private static bool ShouldIgnorePackageFile(string name)
    {
        return string.Equals(name, "[Content_Types].xml", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "_rels/.rels", StringComparison.OrdinalIgnoreCase);
    }

    #endregion Helper methods
}
