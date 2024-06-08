// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal interface IResourceVisitor
{
    bool Visit(IResource resource);
}
