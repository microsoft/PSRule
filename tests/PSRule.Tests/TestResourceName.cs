// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions;

namespace PSRule;

internal sealed class TestResourceName : IResource
{
    public TestResourceName(ResourceId id, ResourceTags? resourceTags = null, ResourceLabels? resourceLabels = null)
    {
        Id = id;
        Name = Id.Name;
        Module = Id.Scope != null && Id.Scope != "." ? Id.Scope : null;
        Tags = resourceTags ?? new ResourceTags();
        Labels = resourceLabels ?? new ResourceLabels();
    }

    public ResourceKind Kind => throw new NotImplementedException();

    public string ApiVersion => throw new NotImplementedException();

    public string Name { get; }

    public ResourceId? Ref => null;

    public ResourceId[] Alias => Array.Empty<ResourceId>();

    public IResourceTags Tags { get; }

    public IResourceLabels Labels { get; }

    public ResourceFlags Flags => ResourceFlags.None;

    public ISourceExtent Extent => throw new NotImplementedException();

    public IResourceHelpInfo Info => throw new NotImplementedException();

    public ResourceId Id { get; }

    public string SourcePath => throw new NotImplementedException();

    public string Module { get; }

    public ISourceFile Source => throw new NotImplementedException();
}
