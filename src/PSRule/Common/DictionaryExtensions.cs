// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSRule
{
    internal static class DictionaryExtensions
    {
        public static bool GetFieldName(this IDictionary dictionary, string fieldName, bool caseSensitive, out object value)
        {
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            foreach (var k in dictionary.Keys)
            {
                if (comparer.Equals(fieldName, k))
                {
                    value = dictionary[k];
                    return true;
                }
            }

            value = null;
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryPopValue(this IDictionary<string, object> dictionary, string key, out object value)
        {
            return dictionary.TryGetValue(key, out value) && dictionary.Remove(key);
        }
    }
}
