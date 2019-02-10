using System;
using System.Collections.Generic;
using System.Text;

namespace PSRule.Rules
{
    /// <summary>
    /// A source file for rule definitions.
    /// </summary>
    public sealed class RuleSource
    {
        public readonly string Path;
        public readonly string ModuleName;

        public RuleSource(string path, string moduleName)
        {
            Path = path;
            ModuleName = moduleName;
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

        public void Add(string[] path, string moduleName)
        {
            if (path == null || path.Length == 0)
            {
                return;
            }

            for (var i = 0; i < path.Length; i++)
            {
                _Source.Add(new RuleSource(path[i], moduleName));
            }
        }

        public RuleSource[] Build()
        {
            return _Source.ToArray();
        }
    }
}
