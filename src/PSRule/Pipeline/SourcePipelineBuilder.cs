// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using System.Reflection;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Pipeline.Output;
using PSRule.Resources;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a list of rule sources for discovery.
/// </summary>
public sealed class SourcePipelineBuilder : ISourcePipelineBuilder, ISourceCommandLineBuilder
{
    private const string SOURCE_FILE_EXTENSION_YAML = ".yaml";
    private const string SOURCE_FILE_EXTENSION_YML = ".yml";
    private const string SOURCE_FILE_EXTENSION_JSON = ".json";
    private const string SOURCE_FILE_EXTENSION_JSONC = ".jsonc";
    private const string SOURCE_FILE_EXTENSION_PS1 = ".ps1";
    private const string SOURCE_FILE_PATTERN = "*.Rule.*";
    private const string RULE_MODULE_TAG = "PSRule-rules";
    private const string DEFAULT_RULE_PATH = ".ps-rule/";

    private readonly Dictionary<string, Source> _Source;
    private readonly IHostContext _HostContext;
    private readonly HostPipelineWriter _Writer;
    private readonly bool _UseDefaultPath;
    private readonly string _LocalPath;
    private readonly RestrictScriptSource _RestrictScriptSource;
    private readonly string _WorkspacePath;

    internal SourcePipelineBuilder(IHostContext hostContext, PSRuleOption option, string localPath = null)
    {
        _Source = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
        _HostContext = hostContext;
        _Writer = new HostPipelineWriter(hostContext, option, ShouldProcess);
        _Writer.EnterScope("[Discovery.Source]");
        _UseDefaultPath = option == null || option.Include == null || option.Include.Path == null;
        _LocalPath = localPath;
        _RestrictScriptSource = option?.Execution?.RestrictScriptSource ?? ExecutionOption.Default.RestrictScriptSource.Value;
        _WorkspacePath = Environment.GetRootedBasePath(null);

        // Include paths from options
        if (!_UseDefaultPath)
            Directory(option.Include.Path);
    }

    /// <inheritdoc/>
    public bool ShouldLoadModule => _HostContext.GetAutoLoadingPreference() == PSModuleAutoLoadingPreference.All;

    #region Logging

    /// <inheritdoc/>
    public void VerboseScanSource(string path)
    {
        Log(PSRuleResources.ScanSource, path);
    }

    /// <inheritdoc/>
    public void VerboseFoundModules(int count)
    {
        Log(PSRuleResources.FoundModules, count);
    }

    /// <inheritdoc/>
    public void VerboseScanModule(string moduleName)
    {
        Log(PSRuleResources.ScanModule, moduleName);
    }

    /// <summary>
    /// Log a message in the verbose stream.
    /// </summary>
    private void Log(string message, params object[] args)
    {
        if (!_Writer.ShouldWriteVerbose())
            return;

        _Writer.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, message, args));
    }

    /// <summary>
    /// Log a message in the debug stream.
    /// </summary>
    private void Debug(string message, params object[] args)
    {
        if (!_Writer.ShouldWriteDebug())
            return;

        _Writer.WriteDebug(string.Format(Thread.CurrentThread.CurrentCulture, message, args));
    }

    #endregion Logging

    /// <inheritdoc/>
    public void Directory(string[] path, bool excludeDefaultRulePath = false)
    {
        if (path == null || path.Length == 0)
            return;

        for (var i = 0; i < path.Length; i++)
            Directory(path[i], excludeDefaultRulePath);
    }

    /// <inheritdoc/>
    public void Directory(string path, bool excludeDefaultRulePath = false)
    {
        if (string.IsNullOrEmpty(path))
            return;

        VerboseScanSource(path);
        path = Environment.GetRootedPath(path);
        var files = GetFiles(path, null, excludeDefaultRulePath, restrictScriptSource: _RestrictScriptSource, workspacePath: _WorkspacePath);
        if (files == null || files.Length == 0)
            return;

        Source(new Source(path, files));
    }

    /// <inheritdoc/>
    public void Module(PSModuleInfo[] module)
    {
        if (module == null || module.Length == 0)
            return;

        for (var i = 0; i < module.Length; i++)
            Module(module[i], dependency: false);
    }

    /// <inheritdoc/>
    public void ModuleByName(string name, string version = null)
    {
        Log($"Looking for module by name: {name}@{version}");

        var basePath = FindModule(name, version) ?? throw ModuleNotFound(name, version);
        var info = LoadManifest(basePath, name) ?? throw ModuleNotFound(name, version);

        VerboseScanModule(info.Name);
        var files = GetFiles(
            path: basePath,
            helpPath: basePath,
            excludeDefaultRulePath: false,
            restrictScriptSource: _RestrictScriptSource,
            moduleName: info.Name,
            workspacePath: _WorkspacePath
        );

        if (files == null || files.Length == 0)
            return;

        Source(new Source(info, files, dependency: false));

        // Import dependencies
        //for (var i = 0; module.RequiredModules != null && i < module.RequiredModules.Count; i++)
        //    Module(module.RequiredModules[i], dependency: true);
    }

    private string FindModule(string name, string version)
    {
        return TryPackagedModule(name, version, out var path) ||
            TryInstalledModule(name, version, out path) ? path : null;
    }

    /// <summary>
    /// Try to find a packaged module found relative to the tool.
    /// </summary>
    private bool TryPackagedModule(string name, string version, out string path)
    {
        path = null;
        if (_LocalPath == null)
            return false;

        Log($"Searching for module in: {_LocalPath}");
        if (!string.IsNullOrEmpty(version))
        {
            path = Environment.GetRootedBasePath(Path.Combine(_LocalPath, "Modules", name, version));
            if (System.IO.Directory.Exists(path))
                return true;
        }

        path = Environment.GetRootedBasePath(Path.Combine(_LocalPath, "Modules", name));
        if (System.IO.Directory.Exists(path))
            return true;

        return System.IO.Directory.Exists(path);
    }

    /// <summary>
    /// Try to find a module installed into PowerShell.
    /// </summary>
    private bool TryInstalledModule(string name, string version, out string path)
    {
        path = null;
        if (!Environment.TryPathEnvironmentVariable("PSModulePath", out var searchPaths))
            return false;

        var unsorted = new List<string>();
        Log("Searching for module in PowerShell search paths.");
        for (var i = 0; i < searchPaths.Length; i++)
        {
            Debug($"Search for module search path: {searchPaths[i]}");
            var searchPath = Environment.GetRootedBasePath(Path.Combine(searchPaths[i], name));

            // Try a specific version.
            if (!string.IsNullOrEmpty(version))
            {
                var versionPath = Path.Combine(searchPath, version);
                var manifestPath = Path.Combine(versionPath, GetManifestName(name));
                if (File.Exists(manifestPath))
                {
                    Debug($"Found module manifest: {manifestPath}");
                    unsorted.Add(versionPath);
                }
                continue;
            }

            // Get other versions.
            if (System.IO.Directory.Exists(searchPath))
            {
                foreach (var versionPath in System.IO.Directory.EnumerateDirectories(searchPath))
                {
                    var manifestPath = Path.Combine(versionPath, GetManifestName(name));
                    if (File.Exists(manifestPath))
                    {
                        Debug($"Found module manifest: {manifestPath}");
                        unsorted.Add(versionPath);
                    }
                }
            }
        }
        if (unsorted.Count == 0)
            return false;

        var sorted = SortModulePath(unsorted);
        if (sorted.Length > 0)
            path = Environment.GetRootedBasePath(sorted[0]);

        return sorted.Length > 0;
    }

    private static string[] SortModulePath(IEnumerable<string> values)
    {
        var results = values.ToArray();
        Array.Sort(results, new ModulePathComparer());
        return results;
    }

    private Source.ModuleInfo LoadManifest(string basePath, string name)
    {
        var path = Path.Combine(basePath, GetManifestName(name));
        if (!File.Exists(path))
            return null;

        Log("Loading manifest for: {0}", basePath);
        using var reader = new StreamReader(path);
        var data = reader.ReadToEnd();
        var ast = System.Management.Automation.Language.Parser.ParseInput(data, out _, out _);
        var hashtable = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false).FirstOrDefault();
        if (hashtable == null || hashtable.SafeGetValue() is not Hashtable manifest)
            return null;

        var version = manifest["ModuleVersion"] as string;
        var guid = manifest["GUID"] as string;
        var companyName = manifest["CompanyName"] as string;
        var privateData = manifest["PrivateData"] as Hashtable;
        var psData = privateData["PSData"] as Hashtable;
        var projectUri = psData["ProjectUri"] as string;
        var prerelease = psData["Prerelease"] as string;

        if (TryRequiredAssemblies(manifest["RequiredAssemblies"], out var requiredAssemblies))
        {
            foreach (var a in requiredAssemblies)
            {
                var assemblyPath = Path.Combine(basePath, a);
                Log("Loading assembly: {0}", assemblyPath);
                Assembly.LoadFile(assemblyPath);
            }
        }
        return new Source.ModuleInfo(basePath, name, version, projectUri, guid, companyName, prerelease);
    }

    private static bool TryRequiredAssemblies(object value, out IEnumerable<string> requiredAssemblies)
    {
        requiredAssemblies = null;
        if (value == null) return false;

        if (value is string s)
            requiredAssemblies = [s];

        if (value is Array array)
            requiredAssemblies = array.OfType<string>().ToArray();

        return requiredAssemblies != null;
    }

    private static string GetManifestName(string name)
    {
        return string.Concat(name, ".psd1");
    }

    /// <summary>
    /// Add a module source
    /// </summary>
    /// <param name="module">The module info.</param>
    /// <param name="dependency">Flags the source as only a dependency.</param>
    private void Module(PSModuleInfo module, bool dependency)
    {
        if (module == null || !IsRuleModule(module))
            return;

        VerboseScanModule(module.Name);
        var files = GetFiles(module.ModuleBase, module.ModuleBase, excludeDefaultRulePath: false, restrictScriptSource: _RestrictScriptSource, workspacePath: _WorkspacePath, moduleName: module.Name);
        if (files == null || files.Length == 0)
            return;

        Source(new Source(new Source.ModuleInfo(module), files, dependency));

        // Import dependencies
        for (var i = 0; module.RequiredModules != null && i < module.RequiredModules.Count; i++)
            Module(module.RequiredModules[i], dependency: true);
    }

    private static bool IsRuleModule(PSModuleInfo module)
    {
        if (module.Tags == null)
            return false;

        foreach (var tag in module.Tags)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(RULE_MODULE_TAG, tag))
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public Source[] Build()
    {
        Default();
        return _Source.Values.ToArray();
    }

    private void Default()
    {
        if (_UseDefaultPath)
            Directory(DEFAULT_RULE_PATH);
    }

    private void Source(Source source)
    {
        if (source == null)
            return;

        // Prefer non-dependencies
        var key = string.Concat(source.Module?.Name, ": ", source.Path);
        if (_Source.ContainsKey(key) && source.Dependency)
            return;

        _Source[key] = source;
    }

    private static SourceFile[] GetFiles(string path, string helpPath, bool excludeDefaultRulePath, RestrictScriptSource restrictScriptSource, string workspacePath, string moduleName = null)
    {
        var rootedPath = Environment.GetRootedPath(path);
        var extension = Path.GetExtension(rootedPath);
        if (IsSourceFile(extension))
        {
            return IncludeFile(rootedPath, helpPath, restrictScriptSource, workspacePath);
        }
        else if (System.IO.Directory.Exists(rootedPath))
        {
            return IncludePath(rootedPath, helpPath, moduleName, excludeDefaultRulePath, restrictScriptSource, workspacePath);
        }
        return null;
    }

    private static bool ShouldInclude(string path)
    {
        return path.EndsWith(".rule.ps1", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".rule.yaml", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".rule.yml", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".rule.json", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".rule.jsonc", StringComparison.OrdinalIgnoreCase);
    }

    private static SourceFile[] IncludeFile(string path, string helpPath, RestrictScriptSource restrictScriptSource, string workspacePath)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(PSRuleResources.SourceNotFound, path);

        var sourceType = GetSourceType(path);
        if (sourceType == SourceType.Script && IgnoreScript(path, restrictScriptSource, workspacePath))
            return null;

        helpPath ??= Path.GetDirectoryName(path);
        return [new(path, null, sourceType, helpPath)];
    }

    private static SourceFile[] IncludePath(string path, string helpPath, string moduleName, bool excludeDefaultRulePath, RestrictScriptSource restrictScriptSource, string workspacePath)
    {
        if (!excludeDefaultRulePath)
        {
            var allFiles = System.IO.Directory.EnumerateFiles(path, SOURCE_FILE_PATTERN, SearchOption.AllDirectories);
            return GetSourceFiles(allFiles, helpPath, moduleName, restrictScriptSource, workspacePath);
        }
        var filteredFiles = FilterFiles(path, SOURCE_FILE_PATTERN, dir => !PathContainsDefaultRulePath(dir));
        return GetSourceFiles(filteredFiles, helpPath, moduleName, restrictScriptSource, workspacePath);
    }

    private static bool PathContainsDefaultRulePath(string path)
    {
        return path.Contains(DEFAULT_RULE_PATH.TrimEnd(Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static SourceFile[] GetSourceFiles(IEnumerable<string> files, string helpPath, string moduleName, RestrictScriptSource restrictScriptSource, string workspacePath)
    {
        var result = new List<SourceFile>();
        foreach (var file in files)
        {
            if (ShouldInclude(file))
            {
                var sourceType = GetSourceType(file);
                if (sourceType == SourceType.Script && IgnoreScript(file, restrictScriptSource, workspacePath))
                    continue;

                helpPath ??= Path.GetDirectoryName(file);
                result.Add(new SourceFile(file, moduleName, sourceType, helpPath));
            }
        }
        return result.ToArray();
    }

    private static bool IgnoreScript(string path, RestrictScriptSource restrictScriptSource, string workspacePath)
    {
        if (restrictScriptSource == RestrictScriptSource.DisablePowerShell) return true;
        if (restrictScriptSource == RestrictScriptSource.Unrestricted) return false;

        path = Environment.GetRootedPath(path);
        return path.StartsWith(workspacePath);
    }

    private static IEnumerable<string> FilterFiles(string path, string filePattern, Func<string, bool> directoryFilter)
    {
        foreach (var file in System.IO.Directory.GetFiles(path, filePattern, SearchOption.TopDirectoryOnly))
            yield return file;

        var filteredDirectories = System.IO.Directory
            .GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(directoryFilter);

        foreach (var directory in filteredDirectories)
        {
            foreach (var file in FilterFiles(directory, filePattern, directoryFilter))
                yield return file;
        }
    }

    private static SourceType GetSourceType(string path)
    {
        var extension = Path.GetExtension(path);
        if (IsYamlFile(extension))
        {
            return SourceType.Yaml;
        }
        else if (IsJsonFile(extension))
        {
            return SourceType.Json;
        }
        return SourceType.Script;
    }

    private static bool IsSourceFile(string extension)
    {
        return extension == SOURCE_FILE_EXTENSION_PS1 || IsYamlFile(extension) || IsJsonFile(extension);
    }

    private static bool IsYamlFile(string extension)
    {
        return extension == SOURCE_FILE_EXTENSION_YAML || extension == SOURCE_FILE_EXTENSION_YML;
    }

    private static bool IsJsonFile(string extension)
    {
        return extension == SOURCE_FILE_EXTENSION_JSON || extension == SOURCE_FILE_EXTENSION_JSONC;
    }

    private bool ShouldProcess(string target, string action)
    {
        return _HostContext == null || _HostContext.ShouldProcess(target, action);
    }

    private static PipelineBuilderException ModuleNotFound(string name, string version)
    {
        return new PipelineBuilderException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ModuleNotFound, name, version));
    }
}
