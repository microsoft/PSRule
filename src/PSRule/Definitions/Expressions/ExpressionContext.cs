// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Definitions.Expressions;

internal sealed class ExpressionContext : IExpressionContext, IBindingContext
{
    private readonly Dictionary<string, PathExpression> _NameTokenCache;

    private List<ResultReason> _Reason;

    internal ExpressionContext(IExpressionContext context, ISourceFile source, ResourceKind kind, ITargetObject current, ResourceId? ruleId = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Source = source;
        LanguageScope = source.Module;
        Kind = kind;
        RuleId = ruleId;
        _NameTokenCache = [];
        Current = current;
    }

    public ISourceFile Source { get; }

    public string LanguageScope { get; }

    public ILogger Logger => Context.Logger!;

    public ResourceKind Kind { get; }

    public ITargetObject Current { get; }

    /// <inheritdoc/>
    public ResourceId? RuleId { get; }

    public IExpressionContext Context { get; }

    public ILanguageScope? Scope => Context.Scope;

    [DebuggerStepThrough]
    void IBindingContext.CachePathExpression(string path, PathExpression expression)
    {
        _NameTokenCache[path] = expression;
    }

    [DebuggerStepThrough]
    bool IBindingContext.GetPathExpression(string path, out PathExpression expression)
    {
        return _NameTokenCache.TryGetValue(path, out expression);
    }

    public bool TryGetScope(object o, out string[]? scope) => Context.TryGetScope(o, out scope);

    public bool IsScope(RunspaceScope scope) => Context.IsScope(scope);

    public void PushScope(RunspaceScope scope)
    {
        Context.PushScope(scope);
        Context.EnterLanguageScope(Source);
    }

    public void PopScope(RunspaceScope scope)
    {
        Context.PopScope(scope);
    }

    public void EnterLanguageScope(ISourceFile file)
    {
        Context.EnterLanguageScope(file);
    }

    public void Reason(IOperand operand, string text, params object[] args)
    {
        if (string.IsNullOrEmpty(text) || !IsScope(RunspaceScope.Rule))
            return;

        AddReason(new ResultReason(Current?.Path, operand, text, args));
    }

    internal ResultReason[] GetReasons()
    {
        return _Reason == null || _Reason.Count == 0 ? [] : [.. _Reason];
    }

    private void AddReason(ResultReason reason)
    {
        _Reason ??= [];

        // Check if the reason already exists
        if (!_Reason.Contains(reason))
            _Reason.Add(reason);
    }

    /// <inheritdoc/>
    public bool TryGetConfigurationValue(string name, out object? value)
    {
        return Context.TryGetConfigurationValue(name, out value);
    }

    public bool TrySelector(ResourceId id, ITargetObject o)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        if (o == null) throw new ArgumentNullException(nameof(o));

        return Context.TrySelector(id, o);
    }
}
