// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using YamlDotNet.Serialization;

namespace PSRule.Definitions;

/// <summary>
/// Additional resource metadata.
/// </summary>
public sealed class ResourceMetadata
{
    /// <summary>
    /// Create an empty set of metadata.
    /// </summary>
    public ResourceMetadata()
    {
        Annotations = new ResourceAnnotations();
        Tags = new ResourceTags();
        Labels = new ResourceLabels();
    }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A non-localized display name for the resource.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// A non-localized description of the resource.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A opaque reference for the resource.
    /// </summary>
    public string Ref { get; set; }

    /// <summary>
    /// Additional aliases for the resource.
    /// </summary>
    public string[] Alias { get; set; }

    /// <summary>
    /// Any resource annotations.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
    public ResourceAnnotations Annotations { get; set; }

    /// <summary>
    /// Any resource tags.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
    public ResourceTags Tags { get; set; }

    /// <summary>
    /// Any taxonomy references.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
    public ResourceLabels Labels { get; set; }

    /// <summary>
    /// A URL to documentation for the resource.
    /// </summary>
    public string Link { get; set; }
}
