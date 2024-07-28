// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Definitions;

/// <summary>
/// 
/// </summary>
public interface IResourceTags : IDictionary<string, string>
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Hashtable ToHashtable();

    /// <summary>
    /// Check if a specific resource tag exists.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Contains(object key, object value);
}
