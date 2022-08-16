// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using PSRule.Pipeline;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Definitions.Expressions
{
    internal interface IExpressionContext : IBindingContext
    {
        string LanguageScope { get; }

        void Reason(IOperand operand, string text, params object[] args);

        object Current { get; }

        RunspaceContext GetContext();
    }

    internal sealed class ExpressionContext : IExpressionContext, IBindingContext
    {
        private readonly Dictionary<string, PathExpression> _NameTokenCache;

        private List<ResultReason> _Reason;

        internal ExpressionContext(SourceFile source, ResourceKind kind, object current)
        {
            Source = source;
            LanguageScope = source.Module;
            Kind = kind;
            _NameTokenCache = new Dictionary<string, PathExpression>();
            Current = current;
        }

        public SourceFile Source { get; }

        public string LanguageScope { get; }

        public ResourceKind Kind { get; }

        public object Current { get; }

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

        internal void Debug(string message, params object[] args)
        {
            if (RunspaceContext.CurrentThread?.Writer == null)
                return;

            RunspaceContext.CurrentThread.Writer.WriteDebug(message, args);
        }

        internal void PushScope(RunspaceScope scope)
        {
            RunspaceContext.CurrentThread.PushScope(scope);
            RunspaceContext.CurrentThread.EnterSourceScope(Source);
        }

        internal void PopScope(RunspaceScope scope)
        {
            RunspaceContext.CurrentThread.PopScope(scope);
        }

        public void Reason(IOperand operand, string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _Reason ??= new List<ResultReason>();
            _Reason.Add(new ResultReason(RunspaceContext.CurrentThread?.TargetObject?.Path, operand, text, args));
        }

        public void Reason(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _Reason ??= new List<ResultReason>();
            _Reason.Add(new ResultReason(RunspaceContext.CurrentThread?.TargetObject?.Path, null, text, args));
        }

        internal ResultReason[] GetReasons()
        {
            return _Reason == null || _Reason.Count == 0 ? Array.Empty<ResultReason>() : _Reason.ToArray();
        }

        public RunspaceContext GetContext()
        {
            return RunspaceContext.CurrentThread;
        }
    }
}
