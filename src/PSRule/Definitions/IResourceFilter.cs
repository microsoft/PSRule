// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal interface IResourceFilter
{
    ResourceKind Kind { get; }

    bool Match(IResource resource);
}
