// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Runtime;

internal sealed class RuleConditionResult : IConditionResult
{
    internal RuleConditionResult(int pass, int count, bool hadErrors)
    {
        Pass = pass;
        Count = count;
        HadErrors = hadErrors;
    }

    /// <inheritdoc/>
    public int Pass { get; }

    /// <inheritdoc/>
    public int Count { get; }

    /// <inheritdoc/>
    public bool HadErrors { get; }
}
