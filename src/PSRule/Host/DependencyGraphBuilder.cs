// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PSRule.Host
{
    internal sealed class DependencyGraphBuilder<T> where T : IDependencyTarget
    {
        private readonly StringComparer _Comparer;
        private readonly Dictionary<string, T> _Targets;
        private readonly Stack<string> _Stack;
        private readonly bool _IncludeDependencies;

        public DependencyGraphBuilder(bool includeDependencies)
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
            _Targets = new Dictionary<string, T>(_Comparer);
            _Stack = new Stack<string>();
            _IncludeDependencies = includeDependencies;
        }

        public void Include(IEnumerable<T> items, Func<T, bool> filter)
        {
            var index = new Dictionary<string, T>(_Comparer);

            // Load items into index
            foreach (var item in items)
            {
                if (index.ContainsKey(item.RuleId))
                    throw new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DuplicateRuleId, item.RuleId));

                index.Add(item.RuleId, item);
            }

            // Include any matching items
            foreach (var item in index.Values)
            {
                if (filter == null || filter(item))
                    Include(ruleId: item.RuleId, parentId: null, index: index);
            }
        }

        public DependencyGraph<T> Build()
        {
            return new DependencyGraph<T>(_Targets.Values.ToArray());
        }

        public T[] GetItems()
        {
            return _Targets.Values.ToArray();
        }

        private void Include(string ruleId, string parentId, IDictionary<string, T> index)
        {
            // Check that the item is not already in the list of targets
            if (_Targets.ContainsKey(ruleId))
                return;

            // Check for circular dependencies
            if (_Stack.Contains(value: ruleId, comparer: _Comparer))
                throw new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DependencyCircularReference, parentId, ruleId));

            try
            {
                _Stack.Push(item: ruleId);
                var item = index[ruleId];

                // Check for dependencies
                if (item.DependsOn != null && _IncludeDependencies)
                {
                    foreach (var d in item.DependsOn)
                    {
                        if (!index.ContainsKey(d))
                            throw new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DependencyNotFound, d, ruleId));

                        // Handle dependencies
                        if (!_Targets.ContainsKey(d))
                            Include(ruleId: d, parentId: ruleId, index: index);
                    }
                }
                _Targets.Add(key: ruleId, value: item);
            }
            finally
            {
                _Stack.Pop();
            }
        }
    }
}
