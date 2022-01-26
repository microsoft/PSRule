// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Pipeline.Output;
using PSRule.Resources;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    public enum SourceType
    {
        Script = 1,

        Yaml = 2,

        Json = 3
    }

    /// <summary>
    /// A source file for rule definitions.
    /// </summary>
    [DebuggerDisplay("{Type}: {Path}")]
    public sealed class SourceFile
    {
        private bool? _Exists;

        internal Source Source;

        [JsonProperty(PropertyName = "path")]
        public string Path { get; }

        [JsonProperty(PropertyName = "moduleName")]
        public string ModuleName { get; }

        [YamlIgnore]
        [JsonIgnore]
        public SourceType Type { get; }

        [YamlIgnore]
        [JsonIgnore]
        public string HelpPath { get; }

        public SourceFile(string path, string moduleName, SourceType type, string helpPath)
        {
            Path = path;
            ModuleName = moduleName;
            Type = type;
            HelpPath = helpPath;
        }

        internal bool Exists()
        {
            if (!_Exists.HasValue)
                _Exists = File.Exists(Path);

            return _Exists.Value;
        }

        internal bool IsDependency()
        {
            return Source.Dependency;
        }
    }

    public sealed class Source
    {
        internal bool Dependency;

        internal readonly ModuleInfo Module;

        internal Source(string path, SourceFile[] file)
        {
            Path = path;
            Module = null;
            File = file;
            Dependency = false;
            SetSource();
        }

        internal Source(ModuleInfo module, SourceFile[] file, bool dependency)
        {
            Path = module.Path;
            Module = module;
            File = file;
            Dependency = dependency;
            SetSource();
        }

        internal sealed class ModuleInfo
        {
            private const string FIELD_BASELINE = "Baseline";
            private const string FIELD_PRERELEASE = "Prerelease";
            private const string FIELD_PSRULE = "PSRule";
            private const string FIELD_PSDATA = "PSData";
            private const string PRERELEASE_SEPARATOR = "-";

            public readonly string Path;
            public readonly string Name;
            public readonly string Baseline;
            public readonly string Version;
            public readonly string ProjectUri;

            public ModuleInfo(PSModuleInfo info)
            {
                Path = info.ModuleBase;
                Name = info.Name;
                Version = info.Version?.ToString();
                ProjectUri = info.ProjectUri?.ToString();
                if (TryPrivateData(info, FIELD_PSDATA, out var psData) && psData.ContainsKey(FIELD_PRERELEASE))
                    Version = string.Concat(Version, PRERELEASE_SEPARATOR, psData[FIELD_PRERELEASE].ToString());
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
        }

        public string Path { get; }

        public SourceFile[] File { get; }

        internal string Scope
        {
            get
            {
                return Module?.Name ?? Runtime.LanguageScope.STANDALONE_SCOPENAME;
            }
        }

        private void SetSource()
        {
            for (var i = 0; File != null && i < File.Length; i++)
                File[i].Source = this;
        }
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class SourcePipelineBuilder
    {
        private const string SourceFileExtension_YAML = ".yaml";
        private const string SourceFileExtension_YML = ".yml";
        private const string SourceFileExtension_JSON = ".json";
        private const string SourceFileExtension_JSONC = ".jsonc";
        private const string SourceFileExtension_PS1 = ".ps1";
        private const string RuleModuleTag = "PSRule-rules";
        private const string DefaultRulePath = ".ps-rule/";

        private readonly Dictionary<string, Source> _Source;
        private readonly HostContext _HostContext;
        private readonly HostPipelineWriter _Writer;
        private readonly bool _UseDefaultPath;

        internal SourcePipelineBuilder(HostContext hostContext, PSRuleOption option)
        {
            _Source = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
            _HostContext = hostContext;
            _Writer = new HostPipelineWriter(hostContext, option);
            _Writer.EnterScope("[Discovery.Source]");
            _UseDefaultPath = option == null || option.Include == null || option.Include.Path == null;

            // Include paths from options
            if (!_UseDefaultPath)
                Directory(option.Include.Path, excludeDefaultRulePath: false);
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
        public void Directory(string[] path, bool excludeDefaultRulePath = true)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Directory(path[i], excludeDefaultRulePath);
        }

        public void Directory(string path, bool excludeDefaultRulePath)
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
                Directory(DefaultRulePath, excludeDefaultRulePath: false);
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
            if (excludeDefaultRulePath)
            {
                var filteredFiles = FilterFiles(
                    path,
                    directoryFilter: dir => !dir.Contains(
                        DefaultRulePath.TrimEnd(Path.AltDirectorySeparatorChar),
                        StringComparison.OrdinalIgnoreCase),
                    filePattern: "*.Rule.*");

                return GetSourceFiles(filteredFiles, helpPath, moduleName);
            }

            var allFiles = System.IO.Directory.EnumerateFiles(path, "*.Rule.*", SearchOption.AllDirectories);

            return GetSourceFiles(allFiles, helpPath, moduleName);
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

        private static IEnumerable<string> FilterFiles(string path, Func<string, bool> directoryFilter, string filePattern)
        {
            foreach (var file in System.IO.Directory.GetFiles(path, filePattern, SearchOption.TopDirectoryOnly))
                yield return file;

            var filteredDirectories = System.IO.Directory
                .GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(directoryFilter);

            foreach (var directory in filteredDirectories)
            {
                foreach (var file in FilterFiles(directory, directoryFilter, filePattern))
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
