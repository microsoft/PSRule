// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions;
using PSRule.Pipeline;

namespace PSRule;

internal sealed class TestResourceName : IResource
{
    public TestResourceName(ResourceId id, ResourceTags resourceTags = null, ResourceLabels resourceLabels = null)
    {
        Id = id;
        Name = Id.Name;
        Module = Id.Scope != null && Id.Scope != "." ? Id.Scope : null;
        Tags = resourceTags ?? new ResourceTags();
        Labels = resourceLabels ?? new ResourceLabels();
    }

    public ResourceKind Kind => throw new System.NotImplementedException();

    public string ApiVersion => throw new System.NotImplementedException();

    public string Name { get; }

    public ResourceId? Ref => null;

    public ResourceId[] Alias => Array.Empty<ResourceId>();

    public ResourceTags Tags { get; }

    public ResourceLabels Labels { get; }

    public ResourceFlags Flags => ResourceFlags.None;

    public ISourceExtent Extent => throw new System.NotImplementedException();

    public IResourceHelpInfo Info => throw new System.NotImplementedException();

    public ResourceId Id { get; }

    public string SourcePath => throw new System.NotImplementedException();

    public string Module { get; }

    public SourceFile Source => throw new System.NotImplementedException();
}
