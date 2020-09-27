// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Rules
{
    public enum RuleSourceType : byte
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
        public readonly string Path;
        public readonly string ModuleName;
        public readonly RuleSourceType Type;
        public readonly string HelpPath;

        public SourceFile(string path, string moduleName, RuleSourceType type, string helpPath)
        {
            Path = path;
            ModuleName = moduleName;
            Type = type;
            HelpPath = helpPath;
        }
    }

    public sealed class Source
    {
        public readonly string Path;
        public readonly SourceFile[] File;
        internal readonly ModuleInfo Module;

        internal Source(string path, SourceFile[] file)
        {
            Path = path;
            Module = null;
            File = file;
        }

        internal Source(ModuleInfo module, SourceFile[] file)
        {
            Path = module.Path;
            Module = module;
            File = file;
        }

        internal sealed class ModuleInfo
        {
            public readonly string Path;
            public readonly string Name;
            public readonly string Baseline;
            public readonly string Version;

            public ModuleInfo(PSModuleInfo info)
            {
                Path = info.ModuleBase;
                Name = info.Name;
                Version = info.Version.ToString();
                if (TryPrivateData(info, "PSRule", out Hashtable moduleData))
                    Baseline = moduleData.ContainsKey("Baseline") ? moduleData["Baseline"] as string : null;

                if (TryPrivateData(info, "PSData", out Hashtable psData) && psData.ContainsKey("Prerelease"))
                    Version = string.Concat(Version, "-", psData["Prerelease"].ToString());
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
    public sealed class RuleSourceBuilder
    {
        private const string SourceFileExtension_YAML = ".yaml";
        private const string SourceFileExtension_YML = ".yaml";
        private const string SourceFileExtension_PS1 = ".ps1";
        private readonly List<Source> _Source;
        private readonly PipelineLogger _Logger;

        internal RuleSourceBuilder(HostContext hostContext)
        {
            _Source = new List<Source>();
            _Logger = new PipelineLogger();
            if (hostContext != null)
            {
                _Logger.UseCommandRuntime(hostContext.CmdletContext);
                _Logger.UseExecutionContext(hostContext.ExecutionContext);
            }
        }

        public RuleSourceBuilder Configure(PSRuleOption option)
        {
            _Logger.Configure(option);
            _Logger.EnterScope("[Discovery.Source]");
            return this;
        }

        public void VerboseScanSource(string path)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ScanSource, path));
        }

        public void VerboseFoundModules(int count)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.FoundModules, count));
        }

        public void VerboseScanModule(string moduleName)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ScanModule, moduleName));
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
            {
                Directory(path[i]);
            }
        }

        public void Directory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            VerboseScanSource(path);
            var files = GetFiles(path, null);

            if (files == null || files.Length == 0)
                return;

            Source(new Source(path, files));
        }

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="module"></param>
        public void Module(PSModuleInfo[] module)
        {
            if (module == null)
                return;

            for (var i = 0; i < module.Length; i++)
            {
                VerboseScanModule(module[i].Name);
                var files = GetFiles(module[i].ModuleBase, module[i].ModuleBase, module[i].Name);

                if (files == null || files.Length == 0)
                    continue;

                Source(new Source(new Source.ModuleInfo(module[i]), file: files));
            }
        }

        public Source[] Build()
        {
            return _Source.ToArray();
        }

        private void Source(Source source)
        {
            _Source.Add(source);
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

        private static RuleSourceType GetSourceType(string path)
        {
            var extension = Path.GetExtension(path);
            return IsYamlFile(extension) ? RuleSourceType.Yaml : RuleSourceType.Script;
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
