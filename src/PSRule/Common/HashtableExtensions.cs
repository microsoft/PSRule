// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSRule
{
    internal static class HashtableExtensions
    {
        internal static IDictionary<string, object> ToDictionary(this Hashtable hashtable)
        {
            return hashtable
                .Cast<DictionaryEntry>()
                .ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                );
        }
    }
}