// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions;

internal interface IRunspaceScopedContext
{
    void PushScope(RunspaceScope scope);

    void PopScope(RunspaceScope scope);

    bool IsScope(RunspaceScope scope);
}
