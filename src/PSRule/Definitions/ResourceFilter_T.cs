// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Match a specific resource type.
/// </summary>
/// <typeparam name="T">The type to use for matching.</typeparam>
internal abstract class ResourceFilter<T> : IResourceFilter where T : class, IResource
{
    public abstract ResourceKind Kind { get; }

    public bool Match(IResource resource)
    {
        return resource is T t && Match(t);
    }

    public abstract bool Match(T resource);
}
