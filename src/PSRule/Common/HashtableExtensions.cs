// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;

namespace PSRule
{
    internal static class HashtableExtensions
    {
        public static IDictionary<string, object> ToDictionary(this Hashtable hashtable)
        {
            return hashtable
                .Cast<DictionaryEntry>()
                .ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                );
        }

        [DebuggerStepThrough]
        public static bool TryGetStringArray(this Hashtable hashtable, string key, out string[] value)
        {
            value = null;
            return hashtable.TryGetValue(key, out var o) && ExpressionHelpers.TryStringOrArray(o, convert: true, value: out value);
        }

        [DebuggerStepThrough]
        public static bool TryGetValue(this Hashtable hashtable, object key, out object value)
        {
            value = null;
            if (!hashtable.ContainsKey(key))
                return false;

            value = hashtable[key];
            return true;
        }
    }
}
