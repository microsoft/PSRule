// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using PSRule.CommandLine.Models;
using PSRule.CommandLine.Resources;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline.Dependencies;
using SemanticVersion = PSRule.Data.SemanticVersion;

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Execute features of the <c>module</c> command through the CLI.
/// </summary>
public sealed class ModuleCommand
{
    private const int ERROR_SUCCESS = 0;

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
        var exitCode = ERROR_SUCCESS;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);

        using var pwsh = CreatePowerShell();

        clientContext.LogVerbose("[PSRule][M] -- Determining modules to restore.");

        // Restore from the lock file.
        foreach (var kv in file.Modules)
        {
            var module = kv.Key;
            var targetVersion = kv.Value.Version;
            if (string.Equals(module, "PSRule", StringComparison.OrdinalIgnoreCase))
                continue;

            // clientContext.LogVerbose(Messages.UsingModule, module, targetVersion.ToString());
            if (IsInstalled(pwsh, module, targetVersion, out var installedVersion, out _) && !operationOptions.Force)
            {
                clientContext.ResolvedModuleVersions[module] = installedVersion.ToShortString();
                clientContext.LogVerbose($"[PSRule][M] -- The module {module} is already installed.");
                continue;
            }

            var idealVersion = await FindVersionAsync(module, null, targetVersion, null, cancellationToken);
            if (idealVersion != null)
            {
                clientContext.ResolvedModuleVersions[module] = idealVersion.ToShortString();
                installedVersion = await InstallVersionAsync(clientContext, module, idealVersion, kv.Value.Integrity, cancellationToken);
            }

            if (pwsh.HadErrors || idealVersion == null || installedVersion == null)
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
                if (IsInstalled(pwsh, includeModule, null, out var installedVersion, out _) &&
                    !operationOptions.Force &&
                    (moduleConstraint == null || moduleConstraint.Accepts(installedVersion)))
                {
                    // invocation.Log(Messages.UsingModule, includeModule, installedVersion.ToString());
                    clientContext.ResolvedModuleVersions[includeModule] = installedVersion.ToShortString();
                    clientContext.LogVerbose($"[PSRule][M] -- The module {includeModule} is already installed.");
                    continue;
                }

                // Find the ideal version.
                var idealVersion = await FindVersionAsync(includeModule, moduleConstraint, null, null, cancellationToken);
                if (idealVersion != null)
                {
                    clientContext.ResolvedModuleVersions[includeModule] = idealVersion.ToShortString();
                    await InstallVersionAsync(clientContext, includeModule, idealVersion, null, cancellationToken);
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

        if (exitCode == ERROR_SUCCESS)
        {
            clientContext.LogVerbose("[PSRule][M] -- All modules are restored and up-to-date.");
        }

        if (exitCode == 0 && operationOptions.WriteOutput)
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
        var exitCode = ERROR_SUCCESS;
        var requires = clientContext.Option.Requires.ToDictionary();
        var existingFile = LockFile.Read(null);
        var file = !operationOptions.Force ? existingFile : new LockFile();
        using var pwsh = CreatePowerShell();

        if (operationOptions.Force)
        {
            clientContext.LogVerbose(Messages.UsingForce);
        }

        // Add for any included modules.
        if (clientContext.Option?.Include?.Module != null && clientContext.Option.Include.Module.Length > 0)
        {
            foreach (var includeModule in clientContext.Option.Include.Module)
            {
                // Skip modules already in the lock unless force is used.
                if (existingFile.Modules.TryGetValue(includeModule, out var lockEntry) && !operationOptions.Force)
                    continue;

                // Get a constraint if set from options.
                var moduleConstraint = requires.TryGetValue(includeModule, out var c) ? c : null;

                // Find the ideal version.
                var idealVersion = lockEntry != null && (moduleConstraint == null || moduleConstraint.Accepts(lockEntry.Version)) ?
                    lockEntry.Version :
                    await FindVersionAsync(includeModule, moduleConstraint, null, null, cancellationToken);

                if (idealVersion == null)
                {
                    clientContext.LogError(Messages.Error_502, includeModule);
                    return ERROR_MODULE_FAILED_TO_FIND;
                }

                if (lockEntry?.Version == idealVersion && !operationOptions.Force)
                    continue;

                if (!IsInstalled(pwsh, includeModule, idealVersion, out _, out var modulePath))
                {
                    await InstallVersionAsync(clientContext, includeModule, idealVersion, null, cancellationToken);
                }

                clientContext.LogVerbose(Messages.UsingModule, includeModule, idealVersion.ToString());
                file.Modules[includeModule] = new LockEntry(idealVersion)
                {
                    Integrity = IntegrityBuilder.Build(clientContext.IntegrityAlgorithm, modulePath ?? GetModulePath(clientContext, includeModule, idealVersion)),
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
        var exitCode = ERROR_SUCCESS;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);
        var pwsh = CreatePowerShell();

        if (exitCode == ERROR_SUCCESS)
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
        var exitCode = ERROR_SUCCESS;
        if (operationOptions.Module == null || operationOptions.Module.Length == 0) return exitCode;

        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);

        if (operationOptions.Force)
        {
            clientContext.LogVerbose(Messages.UsingForce);
        }

        using var pwsh = CreatePowerShell();
        foreach (var module in operationOptions.Module)
        {
            if (!file.Modules.TryGetValue(module, out var item) || operationOptions.Force)
            {
                // Get a constraint if set from options.
                var moduleConstraint = requires.TryGetValue(module, out var c) ? c : ModuleConstraint.Any(module, includePrerelease: operationOptions.Prerelease);

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

                if (!IsInstalled(pwsh, module, idealVersion, out _, out _) && await InstallVersionAsync(clientContext, module, idealVersion, null, cancellationToken) == null)
                {
                    clientContext.LogError(Messages.Error_501, module, idealVersion);
                    return ERROR_MODULE_FAILED_TO_INSTALL;
                }

                clientContext.LogVerbose(Messages.UsingModule, module, idealVersion.ToString());
                item = new LockEntry(idealVersion)
                {
                    IncludePrerelease = operationOptions.Prerelease && !idealVersion.Stable ? true : null,
                    Integrity = IntegrityBuilder.Build(clientContext.IntegrityAlgorithm, GetModulePath(clientContext, module, idealVersion)),
                };
                file.Modules[module] = item;
            }
            else
            {

            }
        }

        file.Write(null);

        if (exitCode == ERROR_SUCCESS)
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
        var exitCode = ERROR_SUCCESS;
        if (operationOptions.Module == null || operationOptions.Module.Length == 0) return exitCode;

        var file = LockFile.Read(null);

        using var pwsh = CreatePowerShell();
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

        if (exitCode == ERROR_SUCCESS)
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
        var exitCode = ERROR_SUCCESS;
        var requires = clientContext.Option.Requires.ToDictionary();
        var file = LockFile.Read(null);
        var filteredModules = operationOptions.Module != null && operationOptions.Module.Length > 0 ? new HashSet<string>(operationOptions.Module, StringComparer.OrdinalIgnoreCase) : null;

        using var pwsh = CreatePowerShell();
        foreach (var kv in file.Modules.Where(m => filteredModules == null || filteredModules.Contains(m.Key)))
        {
            var includePrerelease = kv.Value.IncludePrerelease ?? operationOptions.Prerelease;

            // Get the module constraint.
            var moduleConstraint = ModuleConstraint.Any(kv.Key, includePrerelease: includePrerelease);

            // Use the constraint set in options.
            if (requires.TryGetValue(kv.Key, out var c))
            {
                // Only allow pre-releases if both the constraint and lock file/ context allows it.
                moduleConstraint = !includePrerelease ? c.Stable() : c;
            }

            // Find the ideal version.
            var idealVersion = await FindVersionAsync(kv.Key, moduleConstraint, null, null, cancellationToken);
            if (idealVersion == null)
            {
                clientContext.LogError(Messages.Error_502, kv.Key);
                return ERROR_MODULE_FAILED_TO_FIND;
            }

            if (idealVersion == kv.Value.Version)
                continue;

            if (!IsInstalled(pwsh, kv.Key, idealVersion, out _, out var modulePath) && await InstallVersionAsync(clientContext, kv.Key, idealVersion, null, cancellationToken) == null)
            {
                clientContext.LogError(Messages.Error_501, kv.Key, idealVersion);
                return ERROR_MODULE_FAILED_TO_INSTALL;
            }

            clientContext.LogVerbose(Messages.UsingModule, kv.Key, idealVersion.ToString());

            kv.Value.Version = idealVersion;
            kv.Value.Integrity = IntegrityBuilder.Build(clientContext.IntegrityAlgorithm, modulePath ?? GetModulePath(clientContext, kv.Key, idealVersion));
            kv.Value.IncludePrerelease = (kv.Value.IncludePrerelease.GetValueOrDefault(false) || operationOptions.Prerelease) && !idealVersion.Stable ? true : null;
            file.Modules[kv.Key] = kv.Value;
        }

        file.Write(null);

        if (exitCode == ERROR_SUCCESS)
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
            var installed = IsInstalled(pwsh, kv.Key, kv.Value.Version, out _, out _);
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

                var installed = IsInstalled(pwsh, includeModule, null, out var installedVersion, out _);
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
        context.Console.Out.WriteLine(JsonConvert.SerializeObject(results, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }));
    }

    private static bool IsInstalled(PowerShell pwsh, string module, SemanticVersion.Version? targetVersion, [NotNullWhen(true)] out SemanticVersion.Version? installedVersion, [NotNullWhen(true)] out string? path)
    {
        pwsh.Commands.Clear();
        pwsh.Streams.ClearStreams();
        pwsh.AddCommand("Get-Module")
            .AddParameter(PARAM_NAME, module)
            .AddParameter("ListAvailable");

        var versions = pwsh.Invoke();
        installedVersion = null;
        path = null;
        foreach (var version in versions)
        {
            if (TryModuleInfo(version, out var versionString, out var versionPath) &&
                versionString != null &&
                SemanticVersion.TryParseVersion(versionString, out var v) &&
                v != null &&
                (targetVersion == null || targetVersion.CompareTo(v) == 0) &&
                v.CompareTo(installedVersion) > 0)
            {
                installedVersion = v;
                path = versionPath;
            }
        }
        return installedVersion != null;
    }

    private static bool TryModuleInfo(PSObject value, [NotNullWhen(true)] out string? version, [NotNullWhen(true)] out string? path)
    {
        version = null;
        path = null;
        if (value?.BaseObject is not PSModuleInfo info)
            return false;

        path = info.ModuleBase;
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

    private static async Task<SemanticVersion.Version?> InstallVersionAsync([DisallowNull] ClientContext context, [DisallowNull] string name, [DisallowNull] SemanticVersion.Version version, LockEntryIntegrity? integrity, CancellationToken cancellationToken)
    {
        context.LogVerbose(Messages.RestoringModule, name, version);

        var cache = new SourceCacheContext();
        var logger = new NullLogger();
        var resource = await GetSourceRepositoryAsync();
        var stringVersion = version.ToString();

        var packageVersion = new NuGetVersion(stringVersion);
        using var packageStream = new MemoryStream();

        if (!await resource.CopyNupkgToStreamAsync(
            name,
            packageVersion,
            packageStream,
            cache,
            logger,
            cancellationToken))
            return null;

        using var packageReader = new PackageArchiveReader(packageStream);
        var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

        var modulePath = GetModulePath(context, name, version);
        var tempPath = GetModuleTempPath(context, name, version);

        // Remove existing module.
        if (Directory.Exists(modulePath))
        {
            Directory.Delete(modulePath, true);
        }

        // Remove existing temp module.
        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
        }

        var count = 0;
        var files = packageReader.GetFiles();
        packageReader.CopyFiles(tempPath, files, (name, targetPath, s) =>
        {
            if (ShouldIgnorePackageFile(name))
                return null;

            s.CopyToFile(targetPath);
            count++;

            return targetPath;

        }, logger, cancellationToken);

        // Check module path exists.
        if (!Directory.Exists(tempPath))
            return null;

        if (integrity != null)
        {
            context.LogVerbose("Checking module integrity: {0} -- {1}", name, integrity.Hash);

            var actualIntegrity = IntegrityBuilder.Build(integrity.Algorithm, tempPath);
            if (!string.Equals(actualIntegrity.Hash, integrity.Hash))
            {
                context.LogVerbose("Module integrity check failed: {0} -- {1}", name, actualIntegrity.Hash);

                // Clean up the temp path.
                if (Directory.Exists(tempPath))
                {
                    Retry(3, 1000, () => Directory.Delete(tempPath, true));
                }

                context.LogError(Messages.Error_504, name, version);
                return null;
            }
        }

        var parentDirectory = Directory.GetParent(modulePath)?.FullName;
        if (!Directory.Exists(parentDirectory) && parentDirectory != null)
            Directory.CreateDirectory(parentDirectory);

        // Move the module to the final path.
        Retry(3, 1000, () => Directory.Move(tempPath, modulePath));

        if (!Directory.Exists(modulePath))
            return null;

        context.LogVerbose("Module saved to: {0} -- {1}", name, modulePath);
        return count > 0 ? version : null;
    }

    private static async Task<FindPackageByIdResource> GetSourceRepositoryAsync()
    {
        var source = new PackageSource(POWERSHELL_GALLERY_SOURCE);
        var repository = Repository.Factory.GetCoreV2(source);
        return await repository.GetResourceAsync<FindPackageByIdResource>();
    }

    private static string GetModulePath(ClientContext context, string name, [DisallowNull] SemanticVersion.Version version)
    {
        return Path.Combine(context.CachePath, MODULES_PATH, name, version.ToShortString());
    }

    private static string GetModuleTempPath(ClientContext context, string name, [DisallowNull] SemanticVersion.Version version)
    {
        return Path.Combine(context.CachePath, MODULES_PATH, string.Concat("temp-", name, "-", version.ToShortString()));
    }

    private static bool ShouldIgnorePackageFile(string name)
    {
        return string.Equals(name, "[Content_Types].xml", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "_rels/.rels", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(name) == ".nuspec" ||
            name.StartsWith("package/services/metadata/core-properties/", StringComparison.OrdinalIgnoreCase);
    }

    private static PowerShell CreatePowerShell()
    {
        return PowerShell.Create();
    }

    /// <summary>
    /// Retry an action a number of times with a delay.
    /// </summary>
    /// <param name="retryCount">The number of retries.</param>
    /// <param name="delay">The delay in milliseconds.</param>
    /// <param name="action">The action to attempt.</param>
    private static void Retry(int retryCount, int delay, Action action)
    {
        var attempts = 0;
        while (attempts < retryCount)
        {
            try
            {
                action();
                return;
            }
            catch (Exception)
            {
                attempts++;
                if (attempts >= retryCount)
                    throw;

                Thread.Sleep(delay);
            }
        }
    }

    #endregion Helper methods
}
