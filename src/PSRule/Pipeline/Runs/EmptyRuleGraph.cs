// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Rules;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// An empty rule graph.
/// </summary>
internal sealed class EmptyRuleGraph() : IRuleGraph
{
    public int Count => 0;

    public IEnumerable<IDependencyNode<IRuleBlock>> GetSingleTarget()
    {
        return [];
    }
}
