// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;
using System.Collections.Generic;

namespace PSRule.Definitions.Selectors
{
    internal class SelectorContext : IBindingContext
    {
        private readonly Dictionary<string, NameToken> _NameTokenCache;

        internal SelectorContext()
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
    }
}
