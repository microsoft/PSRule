// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// 
/// </summary>
public interface IResourceLabels : IDictionary<string, string[]>
{
    /// <summary>
    /// Check if the resource label matches.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Contains(string key, string[] value);
}
