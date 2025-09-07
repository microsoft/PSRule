// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

public interface IResourceFilter
{
    ResourceKind Kind { get; }

    bool Match(IResource resource);
}
