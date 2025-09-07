// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using YamlDotNet.Serialization;

namespace PSRule.Definitions;

/// <summary>
/// A base class for resources.
/// </summary>
/// <typeparam name="TSpec">The type for the resource specification.</typeparam>
[DebuggerDisplay("Kind = {Kind}, ApiVersion = {ApiVersion}, Id = {Id}")]
public abstract class Resource<TSpec> where TSpec : Spec, new()
{
    /// <summary>
    /// Create a resource.
    /// </summary>
    protected internal Resource(ResourceKind kind, string apiVersion, ISourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, TSpec spec)
    {
        Kind = kind;
        ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        Info = info;
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Extent = extent;
        Spec = spec ?? throw new ArgumentNullException(nameof(spec));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Name = metadata.Name ?? throw new NullReferenceException(nameof(metadata.Name));
        Id = new ResourceId(Source.Module, Name, ResourceIdKind.Id);
    }

    /// <summary>
    /// The resource identifier for the resource.
    /// </summary>
    [YamlIgnore()]
    public ResourceId Id { get; }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    [YamlIgnore()]
    public string Name { get; }

    /// <summary>
    /// The name of the module where the resource is defined.
    /// </summary>
    [YamlIgnore()]
    public string Module => Source.Module;

    /// <summary>
    /// The file path where the resource is defined.
    /// </summary>
    [YamlIgnore()]
    public ISourceFile Source { get; }

    /// <summary>
    /// Information about the resource.
    /// </summary>
    [YamlIgnore()]
    public IResourceHelpInfo Info { get; }

    /// <summary>
    /// Resource metadata details.
    /// </summary>
    public ResourceMetadata Metadata { get; }

    /// <summary>
    /// The type of resource.
    /// </summary>
    public ResourceKind Kind { get; }

    /// <summary>
    /// The API version of the resource.
    /// </summary>
    public string ApiVersion { get; }

    /// <summary>
    /// The child specification of the resource.
    /// </summary>
    public TSpec Spec { get; }

    /// <summary>
    /// The source location of the resource.
    /// </summary>
    public ISourceExtent Extent { get; }
}
