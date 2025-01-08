// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class SpecFactory
{
    private readonly Dictionary<string, ISpecDescriptor> _Descriptors;

    public SpecFactory()
    {
        _Descriptors = [];
        foreach (var d in Specs.BuiltinTypes)
            With(d);
    }

    public bool TryDescriptor(string apiVersion, string name, out ISpecDescriptor descriptor)
    {
        var fullName = Spec.GetFullName(apiVersion, name);
        return _Descriptors.TryGetValue(fullName, out descriptor);
    }

    public void With<T, TSpec>(string name, string apiVersion) where T : Resource<TSpec>, IResource where TSpec : Spec, new()
    {
        var descriptor = new SpecDescriptor<T, TSpec>(name, apiVersion);
        _Descriptors.Add(descriptor.FullName, descriptor);
    }

    private void With(ISpecDescriptor descriptor)
    {
        _Descriptors.Add(descriptor.FullName, descriptor);
    }
}
