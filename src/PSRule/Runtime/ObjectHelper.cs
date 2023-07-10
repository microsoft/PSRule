// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper class to traverse object properties.
    /// </summary>
    internal static class ObjectHelper
    {
        public static bool GetPath(PSObject targetObject, string path, bool caseSensitive, out object value)
        {
            return GetPath(null, targetObject, path, caseSensitive, out value);
        }

        public static bool GetPath(IBindingContext bindingContext, object targetObject, string path, bool caseSensitive, out object value)
        {
            var expression = GetPathExpression(bindingContext, path);
            return expression.TryGet(targetObject, caseSensitive, out value);
        }

        public static bool GetPath(IBindingContext bindingContext, object targetObject, string path, bool caseSensitive, out object[] value)
        {
            var expression = GetPathExpression(bindingContext, path);
            return expression.TryGet(targetObject, caseSensitive, out value);
        }

        /// <summary>
        /// Get a token for the specified name either by creating or reading from cache.
        /// </summary>
        [DebuggerStepThrough]
        private static PathExpression GetPathExpression(IBindingContext bindingContext, string path)
        {
            // Try to load nameToken from cache
            if (bindingContext == null || !bindingContext.GetPathExpression(path, out var expression))
            {
                expression = PathExpression.Create(path);
                bindingContext?.CachePathExpression(path, expression);
            }
            return expression;
        }
    }
}
