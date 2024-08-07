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
        Context = context;
        Source = source;
        LanguageScope = source.Module;
        Kind = kind;
        _NameTokenCache = new Dictionary<string, PathExpression>();
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
        if (RunspaceContext.CurrentThread?.Writer == null)
            return;

        RunspaceContext.CurrentThread.Writer.WriteDebug(message, args);
    }

    public void PushScope(RunspaceScope scope)
    {
        RunspaceContext.CurrentThread.PushScope(scope);
        RunspaceContext.CurrentThread.EnterLanguageScope(Source);
    }

    public void PopScope(RunspaceScope scope)
    {
        RunspaceContext.CurrentThread.PopScope(scope);
    }

    public void Reason(IOperand operand, string text, params object[] args)
    {
        if (string.IsNullOrEmpty(text) || !RunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule))
            return;

        _Reason ??= [];
        _Reason.Add(new ResultReason(Context.TargetObject?.Path, operand, text, args));
    }

    public void Reason(string text, params object[] args)
    {
        if (string.IsNullOrEmpty(text) || !RunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule))
            return;

        _Reason ??= [];
        _Reason.Add(new ResultReason(Context.TargetObject?.Path, null, text, args));
    }

    internal ResultReason[] GetReasons()
    {
        return _Reason == null || _Reason.Count == 0 ? Array.Empty<ResultReason>() : _Reason.ToArray();
    }
}
