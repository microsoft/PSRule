// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A builder for input paths.
/// </summary>
internal sealed class InputPathBuilder(IPipelineWriter logger, string basePath, string searchPattern, PathFilter filter, PathFilter required)
    : PathBuilder(logger, basePath, searchPattern, filter, required)
{
}
