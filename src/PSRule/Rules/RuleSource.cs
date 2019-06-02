using System.Collections.Generic;

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

        public RuleSourceBuilder()
        {
            _Source = new List<RuleSource>();
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
