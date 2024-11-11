// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Options;

/// <summary>
/// An interface for an option.
/// </summary>
public interface IOption
{
    /// <summary>
    /// Import from a dictionary index by a string key.
    /// </summary>
    /// <param name="dictionary">A dictionary of key value pairs to load the option from.</param>
    void Import(IDictionary<string, object> dictionary);
}
