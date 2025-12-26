// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Rules;

namespace PSRule.Pipeline.Runs;

public interface IRuleGraph
{
    int Count { get; }

    IEnumerable<IDependencyNode<IRuleBlock>> GetSingleTarget();
}
