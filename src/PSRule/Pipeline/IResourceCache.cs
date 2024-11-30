// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

/// <summary>
/// A cache that stores resources.
/// </summary>
internal interface IResourceCache : IEnumerable<IResource>
{
    bool Import(IResource resource);
}
