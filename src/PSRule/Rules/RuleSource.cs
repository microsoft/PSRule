using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A source file for rule definitions.
    /// </summary>
    public sealed class RuleSource
    {
        public readonly string Path;
        public readonly string ModuleName;
        public readonly string[] HelpPath;

        public RuleSource(string path, string moduleName, string[] helpPath = null)
        {
            Path = path;
            ModuleName = moduleName;
            HelpPath = helpPath;
        }
    }

    /// <summary>
    /// A helper to build a list of rule sources for discovery.
    /// </summary>
    public sealed class RuleSourceBuilder
    {
        private readonly List<RuleSource> _Source;
        private readonly PipelineLogger _Logger;

        internal RuleSourceBuilder()
        {
            _Source = new List<RuleSource>();
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

        public void Add(string path, string moduleName, string helpPath)
        {
            if (path == null || path.Length == 0)
            {
                return;
            }
            _Source.Add(new RuleSource(path, moduleName, new string[] { helpPath }));
        }

        public void Add(string path, string helpPath)
        {
            if (path == null)
            {
                return;
            }
            _Source.Add(new RuleSource(path, null, new string[] { helpPath }));
        }

        public RuleSource[] Build()
        {
            return _Source.ToArray();
        }
    }
}
