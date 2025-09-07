// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline.Runs;

internal interface IRunBuilderContext : IRunOverrideContext, IGetLocalizedPathContext, IResourceContext
{

}
