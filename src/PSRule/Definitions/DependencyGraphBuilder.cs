// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Definitions;

internal sealed class DependencyGraphBuilder<T> where T : IDependencyTarget
{
    private readonly LegacyRunspaceContext _Context;
    private readonly IEqualityComparer<ResourceId> _Comparer;
    private readonly Dictionary<ResourceId, T> _Targets;
    private readonly Stack<ResourceId> _Stack;
    private readonly bool _IncludeDependencies;
    private readonly bool _IncludeDisabled;

    public DependencyGraphBuilder(LegacyRunspaceContext context, bool includeDependencies, bool includeDisabled)
    {
        _Context = context;
        _Comparer = ResourceIdEqualityComparer.Default;
        _Targets = new Dictionary<ResourceId, T>(_Comparer);
        _Stack = new Stack<ResourceId>();
        _IncludeDependencies = includeDependencies;
        _IncludeDisabled = includeDisabled;
    }

    public void Include(DependencyTargetCollection<T> index, Func<T, bool> filter)
    {
        // Include any matching items
        foreach (var item in index.GetAll())
        {
            if (item.Dependency)
                continue;

            if (filter == null || filter(item))
                Include(index, item, parentId: null);
            else if (item is RuleBlock)
                _Context.RuleExcluded(item.Id);
        }
    }

    public DependencyGraph<T> Build()
    {
        return new DependencyGraph<T>(GetItems());
    }

    public T[] GetItems()
    {
        return [.. _Targets.Values];
    }

    private void Include(DependencyTargetCollection<T> index, T item, ResourceId? parentId)
    {
        // Check that the item is not already in the list of targets
        if (_Targets.ContainsKey(item.Id))
            return;

        // Check for circular dependencies
        if (_Stack.Contains(item.Id, _Comparer))
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DependencyCircularReference, parentId, item.Id));

        try
        {
            _Stack.Push(item.Id);

            // Check for dependencies
            if (item.DependsOn != null && _IncludeDependencies)
            {
                foreach (var d in item.DependsOn)
                {
                    if (d == null)
                        continue;

                    if (!index.TryGet(d, out var dep, out var kind) || dep == null)
                        throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.DependencyNotFound, d, item.Id));

                    // Dependency is referenced by an alias.
                    if (_Context != null && kind == ResourceIdKind.Alias)
                        _Context.WarnAliasReference(ResourceKind.Rule, item.Id.Value, dep.Id.Value, d.Value);

                    // Handle dependencies.
                    if (!_Targets.ContainsKey(dep.Id))
                        Include(index, dep, parentId: item.Id);
                }
            }
            _Targets.Add(key: item.Id, value: item);
        }
        finally
        {
            _Stack.Pop();
        }
    }
}
