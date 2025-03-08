// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// An interface for implementing a path filter.
/// </summary>
internal interface IPathFilter
{
    /// <summary>
    /// Determine if the specified path matches the filter meaning it met the conditions to be included.
    /// </summary>
    /// <param name="path">The specified path.</param>
    /// <returns>rReturns <c>true</c> if the path should be included.</returns>
    bool Match(string path);
}
