// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule
{
    internal static class ListExtensions
    {
        public static void AddOrInsert<T>(this IList<T> list, int index, T item)
        {
            if (list.Count <= index)
                list.Add(item);
            else
                list.Insert(index, item);
        }

        public static void AddUnique<T>(this IList<T> list, IList<T> other)
        {
            if (other == null || other.Count == 0)
                return;

            for (var i = 0; i < other.Count; i++)
                if (!list.Contains(other[i]))
                    list.Add(other[i]);
        }
    }
}
