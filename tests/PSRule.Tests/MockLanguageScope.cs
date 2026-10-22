// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSRule.Configuration;
using PSRule.Data;
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

    public ITargetBindingResult Bind(ITargetObject targetObject)
    {
        throw new NotImplementedException();
    }

    public void Configure(OptionContext context)
    {
        throw new NotImplementedException();
    }

    public void ConfigureServices(Action<IRuntimeServiceCollection> configure)
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

    public IEnumerable<Type> GetEmitters()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Type> GetConventions()
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

    public IConfiguration ToConfiguration()
    {
        return new InternalConfiguration(new Dictionary<string, object>());
    }

    public bool TryConfigurationValue(string key, out object value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetName(ITargetObject o, out string name, out string path)
    {
        throw new NotImplementedException();
    }

    public bool TryGetOverride(ResourceId id, out RuleOverride propertyOverride)
    {
        throw new NotImplementedException();
    }

    public bool TryGetType(ITargetObject o, out string type, out string path)
    {
        throw new NotImplementedException();
    }

    public void WithFilter(IResourceFilter resourceFilter)
    {
        if (resourceFilter is RuleFilter ruleFilter)
            RuleFilter = ruleFilter;
    }
}
