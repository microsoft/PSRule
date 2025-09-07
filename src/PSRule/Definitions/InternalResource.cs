// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using YamlDotNet.Serialization;

namespace PSRule.Definitions;

/// <summary>
/// A base class for built-in resource types.
/// </summary>
/// <typeparam name="TSpec">The type of the related <seealso cref="Spec"/> for the resource.</typeparam>
public abstract class InternalResource<TSpec> : Resource<TSpec>, IResource, IAnnotated<ResourceAnnotation> where TSpec : Spec, new()
{
    private readonly Dictionary<Type, ResourceAnnotation> _Annotations;

    private protected InternalResource(ResourceKind kind, string apiVersion, ISourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, TSpec spec)
        : base(kind, apiVersion, source, metadata, info, extent, spec)
    {
        _Annotations = [];
        Obsolete = ResourceHelper.IsObsolete(metadata);
        Flags |= ResourceHelper.IsObsolete(metadata) ? ResourceFlags.Obsolete : ResourceFlags.None;
    }

    [YamlIgnore()]
    internal readonly bool Obsolete;

    [YamlIgnore()]
    internal ResourceFlags Flags { get; }

    ResourceKind IResource.Kind => Kind;

    string IResource.ApiVersion => ApiVersion;

    string IResource.Name => Name;

    // Not supported with base resources.
    ResourceId? IResource.Ref => null;

    // Not supported with base resources.
    ResourceId[]? IResource.Alias => null;

    IResourceTags IResource.Tags => Metadata.Tags;

    IResourceLabels IResource.Labels => Metadata.Labels;

    ResourceFlags IResource.Flags => Flags;

    TAnnotation IAnnotated<ResourceAnnotation>.GetAnnotation<TAnnotation>()
    {
        return _Annotations.TryGetValue(typeof(TAnnotation), out var annotation) ? (TAnnotation)annotation : null;
    }

    void IAnnotated<ResourceAnnotation>.SetAnnotation<TAnnotation>(TAnnotation annotation)
    {
        _Annotations[typeof(TAnnotation)] = annotation;
    }
}
