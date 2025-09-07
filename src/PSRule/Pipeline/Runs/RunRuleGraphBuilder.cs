// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A helper to construct a <see cref="RuleGraph"/> for a run.
/// </summary>
internal sealed class RunRuleGraphBuilder(IResourceCache resourceCache, IRunOverrideContext runOverrideContext, IResourceFilter? filter)
{

    /// <summary>
    /// Get a list of filtered rules.
    /// </summary>
    private IEnumerable<IRuleV1> GetFiltered()
    {
        // var blocks = resourceCache.OfType<IRuleV1>().ToRuleDependencyTargetCollection();
        // blocks.AddRange
        //.Where(r => filter == null || filter.Match(r));

        return resourceCache.OfType<IRuleV1>();

        // var rules = resourceCache.OfType<IRuleV1>();
        // var blocks = rules.ToRuleDependencyTargetCollection(context, skipDuplicateName: false);

        // var builder = new DependencyGraphBuilder<RuleBlock>(context, includeDependencies: true, includeDisabled: false);
        // builder.Include(blocks, filter: (b) => Match(context, b));
        // return builder.Build();
    }

    // private
}
