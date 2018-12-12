using System;
using System.Collections.Generic;
using System.Linq;

namespace PSRule.Host
{
    internal sealed class DependencyGraphBuilder<T> where T : IDependencyTarget
    {
        private readonly StringComparer _Comparer;
        private readonly Dictionary<string, T> _Targets;

        public DependencyGraphBuilder()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
            _Targets = new Dictionary<string, T>(_Comparer);
        }

        public void Include(IEnumerable<T> items, Func<T, bool> filter)
        {
            var index = new Dictionary<string, T>(_Comparer);

            // Load items into index
            foreach (var item in items)
            {
                if (index.ContainsKey(item.RuleId))
                {
                    throw new Exception("Name already exists in index");
                }

                index.Add(item.RuleId, item);
            }

            // Include any matching items
            foreach (var item in index.Values)
            {
                if (filter == null || filter(item))
                {
                    Include(item.RuleId, index);
                }
            }
        }

        public DependencyGraph<T> Build()
        {
            return new DependencyGraph<T>(_Targets.Values.ToArray());
        }

        private void Include(string name, IDictionary<string, T> index)
        {
            // Check that the item is not already in the list of targets
            if (_Targets.ContainsKey(name))
            {
                return;
            }

            var item = index[name];

            // Check for dependencies
            if (item.DependsOn != null)
            {
                foreach (var d in item.DependsOn)
                {
                    if (!index.ContainsKey(d))
                    {
                        throw new Exception("Dependency name does not exist in index");
                    }

                    // Handle nested dependencies
                    if (!_Targets.ContainsKey(d))
                    {
                        Include(d, index);
                    }
                }
            }

            _Targets.Add(name, item);
        }
    }
}
