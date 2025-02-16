// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A container for runtime factories.
/// </summary>
internal sealed class RuntimeFactoryContainer(string scope)
{
    private List<IRuntimeFactory> _Factories = [];

    public string Scope { get; } = scope;

    public IEnumerable<IRuntimeFactory> Factories => _Factories;

    internal void AddFactory(IRuntimeFactory factory)
    {
        _Factories.Add(factory);
    }

    public void Configure(IRuntimeFactoryContext context)
    {
        foreach (var factory in _Factories)
        {
            factory.Configure(context);
        }
    }
}

#nullable restore
