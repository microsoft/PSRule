// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
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
            return targetObject.BaseObject is IDictionary dictionary ?
                TryDictionary(dictionary, path, caseSensitive, out value) :
                TryPropertyValue(targetObject, path, caseSensitive, out value);
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

        private static bool TryDictionary(IDictionary dictionary, string key, bool caseSensitive, out object value)
        {
            value = null;
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            foreach (var k in dictionary.Keys)
            {
                if (comparer.Equals(key, k))
                {
                    value = dictionary[k];
                    return true;
                }
            }
            return false;
        }

        private static bool TryPropertyValue(PSObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            value = null;
            var p = targetObject.Properties[propertyName];
            if (p == null)
                return false;

            if (caseSensitive && !StringComparer.Ordinal.Equals(p.Name, propertyName))
                return false;

            value = p.Value;
            return true;
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
                if (bindingContext != null)
                    bindingContext.CachePathExpression(path, expression);
            }
            return expression;
        }
    }
}
