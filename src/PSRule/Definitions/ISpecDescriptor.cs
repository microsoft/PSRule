// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;

namespace PSRule.Definitions;

internal interface ISpecDescriptor
{
    string Name { get; }

    string ApiVersion { get; }

    string FullName { get; }

    Type SpecType { get; }

    IResource CreateInstance(ISourceFile source, ResourceMetadata metadata, CommentMetadata comment, ISourceExtent extent, object spec);
}
