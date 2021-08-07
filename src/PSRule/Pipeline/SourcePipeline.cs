﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline.Output;
using PSRule.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    public enum SourceType
    {
        Script = 1,

        Yaml = 2
    }

    /// <summary>
    /// A source file for rule definitions.
    /// </summary>
    [DebuggerDisplay("{Type}: {Path}")]
    public sealed class SourceFile
    {
        private bool? _Exists;

        internal Source Source;

        public string Path { get; }
        public string ModuleName { get; }
        public SourceType Type { get; }
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
        public string Path { get; }

        public SourceFile[] File { get; }

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

        private void SetSource()
        {
            for (var i = 0; File != null && i < File.Length; i++)
                File[i].Source = this;
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
                if (TryPrivateData(info, FIELD_PSRULE, out Hashtable moduleData))
                    Baseline = moduleData.ContainsKey(FIELD_BASELINE) ? moduleData[FIELD_BASELINE] as string : null;

                if (TryPrivateData(info, FIELD_PSDATA, out Hashtable psData) && psData.ContainsKey(FIELD_PRERELEASE))
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
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class SourcePipelineBuilder
    {
        private const string SourceFileExtension_YAML = ".yaml";
        private const string SourceFileExtension_YML = ".yaml";
        private const string SourceFileExtension_PS1 = ".ps1";
        private const string RuleModuleTag = "PSRule-rules";
        private const string DefaultRulePath = ".ps-rule/";

        private readonly Dictionary<string, Source> _Source;
        private readonly HostContext _HostContext;
        private readonly HostPipelineWriter _Writer;
        private readonly bool _UseDefaultPath;
        private readonly bool _NoSourcesConfigured;

        internal SourcePipelineBuilder(HostContext hostContext, PSRuleOption option)
        {
            _Source = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
            _HostContext = hostContext;
            _Writer = new HostPipelineWriter(hostContext, option);
            _Writer.EnterScope("[Discovery.Source]");
            _UseDefaultPath = option == null || option.Include == null || option.Include.Path == null;
            _NoSourcesConfigured = option == null || option.Include == null || (option.Include.Path == null && option.Include.Module == null);

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

        public void UsePWD()
        {
            if (!_NoSourcesConfigured)
                return;

            Directory(PSRuleOption.GetWorkingPath());
        }

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">A file or directory path containing one or more rule files.</param>
        public void Directory(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Directory(path[i]);
        }

        public void Directory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            VerboseScanSource(path);
            path = PSRuleOption.GetRootedPath(path);
            var files = GetFiles(path, null);
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
            var files = GetFiles(module.ModuleBase, module.ModuleBase, module.Name);
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

        private static SourceFile[] GetFiles(string path, string helpPath, string moduleName = null)
        {
            var rootedPath = PSRuleOption.GetRootedPath(path);
            var extension = Path.GetExtension(rootedPath);
            if (IsSourceFile(extension))
            {
                return IncludeFile(rootedPath, helpPath);
            }
            else if (System.IO.Directory.Exists(rootedPath))
            {
                return IncludePath(rootedPath, helpPath, moduleName);
            }
            return null;
        }

        private static bool ShouldInclude(string path)
        {
            return path.EndsWith(".rule.ps1", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".rule.yaml", System.StringComparison.OrdinalIgnoreCase);
        }

        private static SourceFile[] IncludeFile(string path, string helpPath)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(PSRuleResources.SourceNotFound, path);

            if (helpPath == null)
                helpPath = Path.GetDirectoryName(path);

            return new SourceFile[] { new SourceFile(path, null, GetSourceType(path), helpPath) };
        }

        private static SourceFile[] IncludePath(string path, string helpPath, string moduleName)
        {
            var result = new List<SourceFile>();
            var files = System.IO.Directory.EnumerateFiles(path, "*.Rule.*", SearchOption.AllDirectories);
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

        private static SourceType GetSourceType(string path)
        {
            var extension = Path.GetExtension(path);
            return IsYamlFile(extension) ? SourceType.Yaml : SourceType.Script;
        }

        private static bool IsSourceFile(string extension)
        {
            return extension == SourceFileExtension_PS1 || extension == SourceFileExtension_YAML || extension == SourceFileExtension_YML;
        }

        private static bool IsYamlFile(string extension)
        {
            return extension == SourceFileExtension_YAML || extension == SourceFileExtension_YML;
        }
    }
}
