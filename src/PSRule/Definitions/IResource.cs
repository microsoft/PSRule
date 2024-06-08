// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A resource language block.
/// </summary>
public interface IResource : ILanguageBlock
{
    /// <summary>
    /// The type of resource.
    /// </summary>
    ResourceKind Kind { get; }

    /// <summary>
    /// The API version of the resource.
    /// </summary>
    string ApiVersion { get; }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// An optional reference identifer for the resource.
    /// </summary>
    ResourceId? Ref { get; }

    /// <summary>
    /// Any additional aliases for the resource.
    /// </summary>
    ResourceId[] Alias { get; }

    /// <summary>
    /// Any resource tags.
    /// </summary>
    ResourceTags Tags { get; }

    /// <summary>
    /// Any taxonomy references.
    /// </summary>
    ResourceLabels Labels { get; }

    /// <summary>
    /// Flags for the resource.
    /// </summary>
    ResourceFlags Flags { get; }

    /// <summary>
    /// The source location of the resource.
    /// </summary>
    ISourceExtent Extent { get; }

    /// <summary>
    /// Additional information about the resource.
    /// </summary>
    IResourceHelpInfo Info { get; }
}
