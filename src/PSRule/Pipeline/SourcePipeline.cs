// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using PSRule.Configuration;
using PSRule.Pipeline.Output;
using PSRule.Resources;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to build a list of sources for a PowerShell pipeline.
    /// </summary>
    public interface ISourcePipelineBuilder
    {
        /// <summary>
        /// Determines if PowerShell should automatically load the module.
        /// </summary>
        bool ShouldLoadModule { get; }

        /// <summary>
        /// Log a verbose message for scanning sources.
        /// </summary>
        void VerboseScanSource(string path);

        /// <summary>
        /// Log a verbose message for source modules.
        /// </summary>
        void VerboseFoundModules(int count);

        /// <summary>
        /// Log a verbose message for scanning for modules.
        /// </summary>
        void VerboseScanModule(string moduleName);

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">An array of file or directory paths containing one or more rule files.</param>
        /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
        void Directory(string[] path, bool excludeDefaultRulePath = false);

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">A file or directory path containing one or more rule files.</param>
        /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
        void Directory(string path, bool excludeDefaultRulePath = false);

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="module">The module info.</param>
        void Module(PSModuleInfo[] module);

        /// <summary>
        /// Build a list of sources for executing within PSRule.
        /// </summary>
        /// <returns>A list of sources.</returns>
        Source[] Build();
    }

    /// <summary>
    /// A helper to build a list of sources for a command-line tool pipeline.
    /// </summary>
    public interface ISourceCommandlineBuilder
    {
        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">An array of file or directory paths containing one or more rule files.</param>
        /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
        void Directory(string[] path, bool excludeDefaultRulePath = false);

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">A file or directory path containing one or more rule files.</param>
        /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
        void Directory(string path, bool excludeDefaultRulePath = false);

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        void ModuleByName(string name);

        /// <summary>
        /// Build a list of sources for executing within PSRule.
        /// </summary>
        /// <returns>A list of sources.</returns>
        Source[] Build();
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class SourcePipelineBuilder : ISourcePipelineBuilder, ISourceCommandlineBuilder
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

        internal SourcePipelineBuilder(IHostContext hostContext, PSRuleOption option, string localPath = null)
        {
            _Source = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
            _HostContext = hostContext;
            _Writer = new HostPipelineWriter(hostContext, option, ShouldProcess);
            _Writer.EnterScope("[Discovery.Source]");
            _UseDefaultPath = option == null || option.Include == null || option.Include.Path == null;
            _LocalPath = localPath;

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

        private void Log(string message, params object[] args)
        {
            if (!_Writer.ShouldWriteVerbose())
                return;

            _Writer.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, message, args));
        }

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
            path = PSRuleOption.GetRootedPath(path);
            var files = GetFiles(path, null, excludeDefaultRulePath);
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
        public void ModuleByName(string name)
        {
            var basePath = FindModule(name);
            if (basePath == null)
                throw new PipelineBuilderException(PSRuleResources.ModuleNotFound);

            var info = LoadManifest(basePath);
            if (info == null)
                throw new PipelineBuilderException(PSRuleResources.ModuleNotFound);

            VerboseScanModule(info.Name);
            var files = GetFiles(basePath, basePath, excludeDefaultRulePath: false, info.Name);
            if (files == null || files.Length == 0)
                return;

            Source(new Source(info, files, dependency: false));

            // Import dependencies
            //for (var i = 0; module.RequiredModules != null && i < module.RequiredModules.Count; i++)
            //    Module(module.RequiredModules[i], dependency: true);
        }

        private string FindModule(string name)
        {
            return TryPackagedModule(name, out var path) ||
                TryInstalledModule(name, out path) ? path : null;
        }

        /// <summary>
        /// Try to find a packaged module found relative to the tool.
        /// </summary>
        private bool TryPackagedModule(string name, out string path)
        {
            path = null;
            if (_LocalPath == null)
                return false;

            Log($"Looking for modules in: {_LocalPath}");
            path = PSRuleOption.GetRootedBasePath(Path.Combine(_LocalPath, "Modules", name));
            return System.IO.Directory.Exists(path);
        }

        /// <summary>
        /// Try to find a module installed into PowerShell.
        /// </summary>
        private bool TryInstalledModule(string name, out string path)
        {
            path = null;
            if (!EnvironmentHelper.Default.TryPathEnvironmentVariable("PSModulePath", out var searchPaths))
                return false;

            var unsorted = new List<string>();
            Log("Looking for modules installed to PowerShell.");
            for (var i = 0; i < searchPaths.Length; i++)
            {
                Debug($"Looking for modules search paths: {searchPaths[i]}");
                var searchPath = PSRuleOption.GetRootedBasePath(Path.Combine(searchPaths[i], name));
                if (System.IO.Directory.Exists(searchPath))
                {
                    foreach (var versionPath in System.IO.Directory.EnumerateDirectories(searchPath))
                    {
                        var manifestPath = Path.Combine(versionPath, GetManifestName(name));
                        if (File.Exists(manifestPath))
                            unsorted.Add(versionPath);
                    }
                }
            }
            if (unsorted.Count == 0)
                return false;

            var sorted = SortModulePath(unsorted);
            if (sorted.Length > 0)
                path = sorted[0];

            return sorted.Length > 0;
        }

        private static string[] SortModulePath(IEnumerable<string> values)
        {
            var results = values.ToArray();
            Array.Sort(results, new ModulePathComparer());
            return results;
        }

        private static Source.ModuleInfo LoadManifest(string basePath)
        {
            var name = Path.GetFileName(Path.GetDirectoryName(basePath));
            var path = Path.Combine(basePath, GetManifestName(name));
            if (!File.Exists(path))
                return null;

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

            if (manifest["RequiredAssemblies"] is Array requiredAssemblies)
            {
                foreach (var a in requiredAssemblies.OfType<string>())
                    Assembly.LoadFile(Path.Combine(basePath, a));
            }
            return new Source.ModuleInfo(basePath, name, version, projectUri, guid, companyName, prerelease);
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
            var files = GetFiles(module.ModuleBase, module.ModuleBase, excludeDefaultRulePath: false, module.Name);
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
                if (StringComparer.OrdinalIgnoreCase.Equals(RULE_MODULE_TAG, tag))
                    return true;

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

        private static SourceFile[] GetFiles(string path, string helpPath, bool excludeDefaultRulePath, string moduleName = null)
        {
            var rootedPath = PSRuleOption.GetRootedPath(path);
            var extension = Path.GetExtension(rootedPath);
            if (IsSourceFile(extension))
            {
                return IncludeFile(rootedPath, helpPath);
            }
            else if (System.IO.Directory.Exists(rootedPath))
            {
                return IncludePath(rootedPath, helpPath, moduleName, excludeDefaultRulePath);
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

        private static SourceFile[] IncludeFile(string path, string helpPath)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(PSRuleResources.SourceNotFound, path);

            helpPath ??= Path.GetDirectoryName(path);
            return new SourceFile[] { new SourceFile(path, null, GetSourceType(path), helpPath) };
        }

        private static SourceFile[] IncludePath(string path, string helpPath, string moduleName, bool excludeDefaultRulePath)
        {
            if (!excludeDefaultRulePath)
            {
                var allFiles = System.IO.Directory.EnumerateFiles(path, SOURCE_FILE_PATTERN, SearchOption.AllDirectories);
                return GetSourceFiles(allFiles, helpPath, moduleName);
            }
            var filteredFiles = FilterFiles(path, SOURCE_FILE_PATTERN, dir => !PathContainsDefaultRulePath(dir));
            return GetSourceFiles(filteredFiles, helpPath, moduleName);
        }

        private static bool PathContainsDefaultRulePath(string path)
        {
            return path.Contains(DEFAULT_RULE_PATH.TrimEnd(Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        }

        private static SourceFile[] GetSourceFiles(IEnumerable<string> files, string helpPath, string moduleName)
        {
            var result = new List<SourceFile>();
            foreach (var file in files)
            {
                if (ShouldInclude(file))
                {
                    helpPath ??= Path.GetDirectoryName(file);
                    result.Add(new SourceFile(file, moduleName, GetSourceType(file), helpPath));
                }
            }
            return result.ToArray();
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
    }
}
