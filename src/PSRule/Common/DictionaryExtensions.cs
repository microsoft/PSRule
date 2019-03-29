using System;
using System.Collections;

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
    }
}
