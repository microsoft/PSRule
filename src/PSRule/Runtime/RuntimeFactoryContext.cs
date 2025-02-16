// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A context for initialization of a runtime factory.
/// </summary>
internal sealed class RuntimeFactoryContext(IRuntimeServiceCollection serviceCollection) : IRuntimeFactoryContext
{
    private readonly IRuntimeServiceCollection _ServiceCollection = serviceCollection;

    public void ConfigureServices(Action<IRuntimeServiceCollection> configure)
    {
        configure(_ServiceCollection);
    }
}
