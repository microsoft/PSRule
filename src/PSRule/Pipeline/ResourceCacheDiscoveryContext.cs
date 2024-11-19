// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Define a context used for early stage resource discovery.
/// </summary>
internal sealed class ResourceCacheDiscoveryContext(IPipelineWriter writer) : IResourceDiscoveryContext
{
    public IPipelineWriter Writer { get; } = writer;

    public ISourceFile? Source { get; private set; }

    public void EnterLanguageScope(ISourceFile file)
    {
        if (!file.Exists())
            throw new FileNotFoundException(PSRuleResources.ScriptNotFound, file.Path);

        Source = file;
    }

    public void ExitLanguageScope(ISourceFile file)
    {
        Source = null;
    }

    public void PopScope(RunspaceScope scope)
    {

    }

    public void PushScope(RunspaceScope scope)
    {

    }
}

#nullable restore
