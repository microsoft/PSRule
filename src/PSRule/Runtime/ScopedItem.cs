// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime.Scripting;

namespace PSRule.Runtime;

/// <summary>
/// A base class for scoped context variables used internally by PSRule.
/// </summary>
public abstract class ScopedItem
{
    private readonly IRunspaceContext? _Context;

    internal ScopedItem()
    {

    }

    internal ScopedItem(IRunspaceContext context)
    {
        _Context = context;
    }

    #region Helper methods

    internal void RequireScope(RunspaceScope scope)
    {
        if (GetRunspaceContext().IsScope(scope) == true)
            return;

        throw new RuntimeScopeException();
    }

    internal IRunspaceContext? GetRunspaceContext()
    {
        return _Context;
    }

    internal LegacyRunspaceContext? GetResourceContext()
    {
        return _Context.ResourceContext as LegacyRunspaceContext;
    }

    #endregion Helper methods
}
