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
    public interface ISourcePipelineBuilder
    {
        bool ShouldLoadModule { get; }

        void VerboseScanSource(string path);

        void VerboseFoundModules(int count);

        void VerboseScanModule(string moduleName);

        void Directory(string[] path, bool excludeDefaultRulePath = false);

        void Directory(string path, bool excludeDefaultRulePath = false);

        void Module(PSModuleInfo[] module);

        Source[] Build();
    }

    public interface ISourceCommandlineBuilder
    {
        void Directory(string[] path, bool excludeDefaultRulePath = false);

        void Directory(string path, bool excludeDefaultRulePath = false);

        void ModuleByName(string name);

        Source[] Build();
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class SourcePipelineBuilder : ISourcePipelineBuilder, ISourceCommandlineBuilder
    {
        private const string SourceFileExtension_YAML = ".yaml";
        private const string SourceFileExtension_YML = ".yml";
        private const string SourceFileExtension_JSON = ".json";
        private const string SourceFileExtension_JSONC = ".jsonc";
        private const string SourceFileExtension_PS1 = ".ps1";
        private const string RuleModuleTag = "PSRule-rules";
        private const string DefaultRulePath = ".ps-rule/";

        private readonly Dictionary<string, Source> _Source;
        private readonly IHostContext _HostContext;
        private readonly HostPipelineWriter _Writer;
        private readonly bool _UseDefaultPath;
        private readonly string _LocalPath;

        internal SourcePipelineBuilder(IHostContext hostContext, PSRuleOption option)
        {
            _Source = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
            _HostContext = hostContext;
            _Writer = new HostPipelineWriter(hostContext, option);
            _Writer.EnterScope("[Discovery.Source]");
            _UseDefaultPath = option == null || option.Include == null || option.Include.Path == null;
            _LocalPath = PSRuleOption.GetRootedBasePath(Path.GetDirectoryName(typeof(SourcePipelineBuilder).Assembly.Location));

            // Include paths from options
            if (!_UseDefaultPath)
                Directory(option.Include.Path);
        }

        public bool ShouldLoadModule => _HostContext.GetAutoLoadingPreference() == PSModuleAutoLoadingPreference.All;

        public void VerboseScanSource(string path)
        {
            if (!_Writer.ShouldWriteVerbose())
                return;

            _Writer.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ScanSource, path));
        }

        public void VerboseFoundModules(int count)
        {
            if (!_Writer.ShouldWriteVerbose())
                return;

            _Writer.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.FoundModules, count));
        }

        public void VerboseScanModule(string moduleName)
        {
            if (!_Writer.ShouldWriteVerbose())
                return;

            _Writer.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ScanModule, moduleName));
        }

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">A file or directory path containing one or more rule files.</param>
        public void Directory(string[] path, bool excludeDefaultRulePath = false)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Directory(path[i], excludeDefaultRulePath);
        }

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

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="module">The module info.</param>
        public void Module(PSModuleInfo[] module)
        {
            if (module == null || module.Length == 0)
                return;

            for (var i = 0; i < module.Length; i++)
                Module(module[i], dependency: false);
        }

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        public void ModuleByName(string name)
        {
            var basePath = FindModule(name);
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
            return PSRuleOption.GetRootedBasePath(Path.Combine(_LocalPath, "Modules", name));
        }

        private static Source.ModuleInfo LoadManifest(string basePath)
        {
            var name = Path.GetFileName(Path.GetDirectoryName(basePath));
            var path = Path.Combine(basePath, string.Concat(name, ".psd1"));
            if (!File.Exists(path))
                return null;

            var reader = new StreamReader(path);
            var data = reader.ReadToEnd();
            var ast = System.Management.Automation.Language.Parser.ParseInput(data, out _, out _);
            var hashtable = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false).FirstOrDefault();
            var manifest = hashtable.SafeGetValue() as Hashtable;
            if (manifest == null)
                return null;

            var version = manifest["ModuleVersion"] as string;
            var guid = manifest["GUID"] as string;
            var companyName = manifest["CompanyName"] as string;
            var privateData = manifest["PrivateData"] as Hashtable;
            var psData = privateData["PSData"] as Hashtable;
            var projectUri = psData["ProjectUri"] as string;
            var prerelease = psData["Prerelease"] as string;
            var requiredAssemblies = manifest["RequiredAssemblies"] as Array;

            foreach (var a in requiredAssemblies.OfType<string>())
                Assembly.LoadFile(Path.Combine(basePath, a));

            return new Source.ModuleInfo(basePath, name, version, projectUri, guid, companyName, prerelease);
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
                if (StringComparer.OrdinalIgnoreCase.Equals(RuleModuleTag, tag))
                    return true;

            return false;
        }

        public Source[] Build()
        {
            Default();
            return _Source.Values.ToArray();
        }

        private void Default()
        {
            if (_UseDefaultPath)
                Directory(DefaultRulePath);
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

            if (helpPath == null)
                helpPath = Path.GetDirectoryName(path);

            return new SourceFile[] { new SourceFile(path, null, GetSourceType(path), helpPath) };
        }

        private static SourceFile[] IncludePath(string path, string helpPath, string moduleName, bool excludeDefaultRulePath)
        {
            if (!excludeDefaultRulePath)
            {
                var allFiles = System.IO.Directory.EnumerateFiles(path, "*.Rule.*", SearchOption.AllDirectories);
                return GetSourceFiles(allFiles, helpPath, moduleName);
            }

            var filteredFiles = FilterFiles(path, "*.Rule.*", dir => !PathContainsDefaultRulePath(dir));
            return GetSourceFiles(filteredFiles, helpPath, moduleName);
        }

        private static bool PathContainsDefaultRulePath(string path)
        {
            return path.Contains(DefaultRulePath.TrimEnd(Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        }

        private static SourceFile[] GetSourceFiles(IEnumerable<string> files, string helpPath, string moduleName)
        {
            var result = new List<SourceFile>();

            foreach (var file in files)
            {
                if (ShouldInclude(file))
                {
                    if (helpPath == null)
                        helpPath = Path.GetDirectoryName(file);

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
            return extension == SourceFileExtension_PS1 || IsYamlFile(extension) || IsJsonFile(extension);
        }

        private static bool IsYamlFile(string extension)
        {
            return extension == SourceFileExtension_YAML || extension == SourceFileExtension_YML;
        }

        private static bool IsJsonFile(string extension)
        {
            return extension == SourceFileExtension_JSON || extension == SourceFileExtension_JSONC;
        }
    }
}
