// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Annotations;

/// <summary>
/// Metadata properties that can be exposed by comment help.
/// </summary>
[DebuggerDisplay("Synopsis = {Synopsis}")]
internal sealed class CommentMetadata
{
    public string? Synopsis;
}
