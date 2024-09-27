// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;
using PSRule.Runtime.Binding;

namespace PSRule;

internal sealed class MockLanguageScope : ILanguageScope
{
    internal RuleFilter RuleFilter;

    public MockLanguageScope(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public BindingOption Binding => throw new NotImplementedException();

    public string[] Culture => throw new NotImplementedException();

    public void AddService(string name, object service)
    {

    }

    public ITargetBindingResult Bind(TargetObject targetObject)
    {
        throw new NotImplementedException();
    }

    public ITargetBindingResult Bind(object targetObject)
    {
        throw new NotImplementedException();
    }

    public void Configure(OptionContext context)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {

    }

    public StringComparer GetBindingComparer()
    {
        throw new NotImplementedException();
    }

    public IResourceFilter GetFilter(ResourceKind kind)
    {
        throw new NotImplementedException();
    }

    public object GetService(string name)
    {
        throw new NotImplementedException();
    }

    public bool TryConfigurationValue(string key, out object value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetName(object o, out string name, out string path)
    {
        throw new NotImplementedException();
    }

    public bool TryGetType(object o, out string type, out string path)
    {
        throw new NotImplementedException();
    }

    public void WithFilter(IResourceFilter resourceFilter)
    {
        if (resourceFilter is RuleFilter ruleFilter)
            RuleFilter = ruleFilter;
    }
}
