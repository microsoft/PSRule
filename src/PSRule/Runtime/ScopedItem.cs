// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime;

/// <summary>
/// A base class for scoped context variables used internally by PSRule.
/// </summary>
public abstract class ScopedItem
{
    private readonly LegacyRunspaceContext _Context;

    internal ScopedItem()
    {

    }

    internal ScopedItem(LegacyRunspaceContext context)
    {
        _Context = context;
    }

    #region Helper methods

    internal void RequireScope(RunspaceScope scope)
    {
        if (GetContext().IsScope(scope))
            return;

        throw new RuntimeScopeException();
    }

    internal LegacyRunspaceContext GetContext()
    {
        return _Context ?? LegacyRunspaceContext.CurrentThread;
    }

    #endregion Helper methods
}
