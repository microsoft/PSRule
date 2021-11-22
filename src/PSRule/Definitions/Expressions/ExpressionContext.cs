// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions
{
    internal interface IExpressionContext : IBindingContext
    {
        string LanguageScope { get; }

        void Reason(IOperand operand, string text, params object[] args);

        void Reason(string text, params object[] args);

        RunspaceContext GetContext();
    }

    internal sealed class ExpressionContext : IExpressionContext, IBindingContext
    {
        private readonly Dictionary<string, NameToken> _NameTokenCache;

        private List<string> _Reason;

        internal ExpressionContext(string languageScope)
        {
            LanguageScope = languageScope;
            _NameTokenCache = new Dictionary<string, NameToken>();
        }

        public string LanguageScope { get; }

        [DebuggerStepThrough]
        void IBindingContext.CacheNameToken(string expression, NameToken nameToken)
        {
            _NameTokenCache[expression] = nameToken;
        }

        [DebuggerStepThrough]
        bool IBindingContext.GetNameToken(string expression, out NameToken nameToken)
        {
            return _NameTokenCache.TryGetValue(expression, out nameToken);
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
        }

        internal void PopScope(RunspaceScope scope)
        {
            RunspaceContext.CurrentThread.PopScope(scope);
        }

        public void Reason(IOperand operand, string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (_Reason == null)
                _Reason = new List<string>();

            if (args == null || args.Length == 0)
                _Reason.Add(string.Concat(operand.ToString(), ": ", text));
            else
                _Reason.Add(string.Concat(operand.ToString(), ": ", string.Format(Thread.CurrentThread.CurrentCulture, text, args)));
        }

        public void Reason(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (_Reason == null)
                _Reason = new List<string>();

            if (args == null || args.Length == 0)
                _Reason.Add(text);
            else
                _Reason.Add(string.Format(Thread.CurrentThread.CurrentCulture, text, args));
        }

        internal string[] GetReasons()
        {
            if (_Reason == null || _Reason.Count == 0)
                return Array.Empty<string>();

            return _Reason.ToArray();
        }

        public RunspaceContext GetContext()
        {
            return RunspaceContext.CurrentThread;
        }
    }
}
