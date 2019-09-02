using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

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

            public ModuleInfo(PSModuleInfo info)
            {
                Path = info.ModuleBase;
                Name = info.Name;
                if (info.PrivateData is Hashtable privateData && privateData.ContainsKey("PSRule") && privateData["PSRule"] is Hashtable moduleData)
                    Baseline = moduleData.ContainsKey("Baseline") ? moduleData["Baseline"] as string : null;
            }
        }
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class RuleSourceBuilder
    {
        private readonly List<Source> _Source;
        private readonly PipelineLogger _Logger;

        internal RuleSourceBuilder()
        {
            _Source = new List<Source>();
            _Logger = new PipelineLogger();
        }

        public RuleSourceBuilder Configure(PSRuleOption option)
        {
            _Logger.Configure(option);
            _Logger.EnterScope("[Discovery.Source]");
            return this;
        }

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _Logger.UseExecutionContext(executionContext);
        }

        public void VerboseScanSource(string path)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(PSRuleResources.ScanSource, path));
        }

        public void VerboseFoundModules(int count)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(PSRuleResources.FoundModules, count));
        }

        public void VerboseScanModule(string moduleName)
        {
            if (!_Logger.ShouldWriteVerbose())
            {
                return;
            }
            _Logger.WriteVerbose(string.Format(PSRuleResources.ScanModule, moduleName));
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

        private SourceFile[] GetFiles(string path, string helpPath, string moduleName = null)
        {
            var result = new List<SourceFile>();
            var rootedPath = PSRuleOption.GetRootedPath(path);
            var extension = Path.GetExtension(rootedPath);
            if (extension == ".ps1" || extension == ".yaml" || extension == ".yml")
            {
                if (!File.Exists(rootedPath))
                    throw new FileNotFoundException(PSRuleResources.SourceNotFound, rootedPath);

                if (helpPath == null)
                    helpPath = Path.GetDirectoryName(rootedPath);

                result.Add(new SourceFile(rootedPath, null, GetSourceType(rootedPath), helpPath));
            }
            else if (System.IO.Directory.Exists(rootedPath))
            {
                var files = System.IO.Directory.EnumerateFiles(rootedPath, "*.rule.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Include(file))
                    {
                        if (helpPath == null)
                            helpPath = Path.GetDirectoryName(file);

                        result.Add(new SourceFile(file, moduleName, GetSourceType(file), helpPath));
                    }
                }
            }
            else
            {
                return null;
            }
            return result.ToArray();
        }

        private bool Include(string path)
        {
            return path.EndsWith(".rule.ps1", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".rule.yaml", System.StringComparison.OrdinalIgnoreCase);
        }

        private RuleSourceType GetSourceType(string path)
        {
            var extension = Path.GetExtension(path);
            return (extension == ".yaml" || extension == ".yml") ? RuleSourceType.Yaml : RuleSourceType.Script;
        }
    }
}
