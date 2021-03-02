// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Common
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
    }
}
