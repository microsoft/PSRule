// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PSRule.Definitions.Expressions
{
    internal class ExpressionContext : IBindingContext
    {
        private readonly Dictionary<string, NameToken> _NameTokenCache;

        private List<string> _Reason;

        internal ExpressionContext()
        {
            _NameTokenCache = new Dictionary<string, NameToken>();
        }

        void IBindingContext.CacheNameToken(string expression, NameToken nameToken)
        {
            _NameTokenCache[expression] = nameToken;
        }

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

        internal void Reason(string text, params object[] args)
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
    }
}
