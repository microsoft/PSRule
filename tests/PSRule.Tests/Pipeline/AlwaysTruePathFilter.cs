// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
///  A path filter for testing that always returns true.
/// </summary>
internal sealed class AlwaysTruePathFilter : IPathFilter
{
    public bool Match(string path)
    {
        return true;
    }
}
