// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;

namespace PSRule.Runtime;

#nullable enable

internal interface IMetaResourceDiscoveryContext
{
    IPipelineWriter Writer { get; }

    void EnterLanguageScope(ISourceFile file);

    void ExitLanguageScope(ISourceFile file);

    void PushScope(RunspaceScope scope);

    void PopScope(RunspaceScope scope);
}

#nullable restore
