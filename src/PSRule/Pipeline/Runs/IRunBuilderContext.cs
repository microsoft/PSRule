// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A context that is used when building a run.
/// </summary>
internal interface IRunBuilderContext : IRunOverrideContext, IGetLocalizedPathContext, IResourceContext
{
    /// <summary>
    /// Determine if the resource should be included in the run.
    /// </summary>
    /// <param name="resource">The resource to match.</param>
    /// <returns>Returns <c>true</c> if the resource should be included in the run; otherwise, <c>false</c>.</returns>
    bool Match(IResource resource);
}
