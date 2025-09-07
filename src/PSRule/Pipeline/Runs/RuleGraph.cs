// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Rules;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A graph of rules used for a run.
/// </summary>
internal sealed class RuleGraph(DependencyGraph<IRuleBlock> graph) : IRuleGraph
{
    public IEnumerable<IDependencyNode<IRuleBlock>> GetSingleTarget()
    {
        return graph.GetSingleTarget();
    }

    public int Count => graph.Count;
}
