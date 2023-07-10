// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;

namespace PSRule
{
    /// <summary>
    /// Extension methods for <see cref="Hashtable"/>.
    /// </summary>
    public static class HashtableExtensions
    {
        /// <summary>
        /// Map the hashtable into a dictionary string a string key.
        /// </summary>
        [DebuggerStepThrough]
        public static IDictionary<string, object> IndexByString(this Hashtable hashtable, bool ignoreCase = true)
        {
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var index = new Dictionary<string, object>(comparer);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
        }
    }
}
