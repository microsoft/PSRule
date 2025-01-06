// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Definitions.Expressions;

internal sealed class ExpressionContext : IExpressionContext, IBindingContext
{
    private readonly Dictionary<string, PathExpression> _NameTokenCache;

    private List<ResultReason> _Reason;

    internal ExpressionContext(RunspaceContext context, ISourceFile source, ResourceKind kind, object current)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Source = source;
        LanguageScope = source.Module;
        Kind = kind;
        _NameTokenCache = [];
        Current = current;
    }

    public ISourceFile Source { get; }

    public string LanguageScope { get; }

    public ResourceKind Kind { get; }

    public object Current { get; }

    public RunspaceContext Context { get; }

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

    public void Debug(string message, params object[] args)
    {
        if (Context.Writer == null)
            return;

        Context.Writer.WriteDebug(message, args);
    }

    public void PushScope(RunspaceScope scope)
    {
        Context.PushScope(scope);
        Context.EnterLanguageScope(Source);
    }

    public void PopScope(RunspaceScope scope)
    {
        Context.PopScope(scope);
    }

    public void Reason(IOperand operand, string text, params object[] args)
    {
        if (string.IsNullOrEmpty(text) || !Context.IsScope(RunspaceScope.Rule))
            return;

        AddReason(new ResultReason(Context.TargetObject?.Path, operand, text, args));
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
}
