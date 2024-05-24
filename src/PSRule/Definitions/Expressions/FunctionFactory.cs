// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal sealed class FunctionFactory
{
    private readonly Dictionary<string, IFunctionDescriptor> _Descriptors;

    public FunctionFactory()
    {
        _Descriptors = new Dictionary<string, IFunctionDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in Functions.Builtin)
            With(d);
    }

    public bool TryDescriptor(string name, out IFunctionDescriptor descriptor)
    {
        return _Descriptors.TryGetValue(name, out descriptor);
    }

    public void With(IFunctionDescriptor descriptor)
    {
        _Descriptors.Add(descriptor.Name, descriptor);
    }
}
