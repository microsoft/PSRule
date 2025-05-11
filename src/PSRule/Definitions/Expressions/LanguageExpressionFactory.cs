// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal sealed class LanguageExpressionFactory
{
    private readonly Dictionary<string, ILanguageExpressionDescriptor> _Descriptors;

    public LanguageExpressionFactory()
    {
        _Descriptors = new Dictionary<string, ILanguageExpressionDescriptor>(LanguageExpressions.Builtin.Length, StringComparer.OrdinalIgnoreCase);
        foreach (var d in LanguageExpressions.Builtin)
            With(d);
    }

    public bool TryDescriptor(string name, out ILanguageExpressionDescriptor? descriptor)
    {
        descriptor = null;
        return !string.IsNullOrEmpty(name) &&
            _Descriptors.TryGetValue(name, out descriptor);
    }

    public bool IsSubselector(string name)
    {
        return name == "where";
    }

    public bool IsOperator(string name)
    {
        return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Operator;
    }

    public bool IsCondition(string name)
    {
        return TryDescriptor(name, out var d) && d != null && d.Type == LanguageExpressionType.Condition;
    }

    public bool IsFunction(string name)
    {
        return TryDescriptor(name, out var d) &&
            d != null && d.Type == LanguageExpressionType.Function;
    }

    private void With(ILanguageExpressionDescriptor descriptor)
    {
        _Descriptors.Add(descriptor.Name, descriptor);
    }
}
