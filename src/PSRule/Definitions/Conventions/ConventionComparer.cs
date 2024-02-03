// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.Conventions;

/// <summary>
/// Orders conventions by the order they are specified.
/// </summary>
internal sealed class ConventionComparer : IComparer<IConvention>
{
    private readonly RunspaceContext _Context;

    internal ConventionComparer(RunspaceContext context)
    {
        _Context = context;
    }

    public int Compare(IConvention x, IConvention y)
    {
        return _Context.Pipeline.GetConventionOrder(x) - _Context.Pipeline.GetConventionOrder(y);
    }
}
