// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// Define a context used for early stage resource discovery.
/// </summary>
internal sealed class ResourceCacheDiscoveryContext(IPipelineWriter writer) : IResourceDiscoveryContext
{
    public IPipelineWriter Writer { get; } = writer;

    public void EnterLanguageScope(ISourceFile file)
    {

    }

    public void ExitLanguageScope(ISourceFile file)
    {

    }

    public void PopScope(RunspaceScope scope)
    {

    }

    public void PushScope(RunspaceScope scope)
    {

    }
}
