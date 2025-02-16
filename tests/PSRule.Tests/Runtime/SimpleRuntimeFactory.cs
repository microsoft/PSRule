// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Emitters;

namespace PSRule.Runtime;

[RuntimeFactory]
public sealed class SimpleRuntimeFactory : IRuntimeFactory
{
    public SimpleRuntimeFactory()
    {

    }

    public void Configure(IRuntimeFactoryContext context)
    {
        context.ConfigureServices(AddServices);
    }

    private void AddServices(IRuntimeServiceCollection collection)
    {
        collection.AddService<IEmitter, CustomEmitter>();
    }
}
