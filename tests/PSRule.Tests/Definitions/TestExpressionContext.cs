// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Definitions;

public sealed class TestExpressionContext(PSRuleOption option, RunspaceScope runspaceScope) : IExpressionContext
{
    private ILanguageScopeSet _LanguageScopeSet = SetupLanguageScope();

    public ResourceKind Kind => ResourceKind.None;

    public ISourceFile Source => throw new System.NotImplementedException();

    public ILogger Logger => NullLogger.Instance;

    public ITargetObject Current => throw new System.NotImplementedException();

    public ResourceId? RuleId => throw new System.NotImplementedException();

    ILanguageScope IExpressionContext.Scope => _LanguageScopeSet.Get().First();

    public string LanguageScope => throw new System.NotImplementedException();

    public void PopScope(RunspaceScope scope)
    {
        // throw new System.NotImplementedException();
    }

    public void PushScope(RunspaceScope scope)
    {
        // throw new System.NotImplementedException();
    }

    public void Reason(IOperand operand, string text, params object[] args)
    {

    }

    public bool TryGetConfigurationValue(string name, out object value)
    {
        return option.Configuration.TryGetValue(name, out value);
    }

    void IBindingContext.CachePathExpression(string path, PathExpression expression)
    {
        throw new System.NotImplementedException();
    }

    bool IBindingContext.GetPathExpression(string path, out PathExpression expression)
    {
        throw new System.NotImplementedException();
    }

    private static ILanguageScopeSet SetupLanguageScope()
    {
        var option = new OptionContext(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, null);
        option.Binding = new BindingOption
        {
            IgnoreCase = true,
        };
        var scopeSet = new LanguageScopeSetBuilder().Build();
        foreach (var scope in scopeSet.Get())
        {
            scope.Configure(option);
        }
        return scopeSet;
    }

    public bool IsScope(RunspaceScope scope)
    {
        return scope == runspaceScope;
    }

    public void EnterLanguageScope(ISourceFile file)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetScope(object o, out string[] scope)
    {
        throw new System.NotImplementedException();
    }

    public bool TrySelector(ResourceId id, ITargetObject o)
    {
        throw new System.NotImplementedException();
    }
}
