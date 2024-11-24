// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A map of <typeparamref name="TValue"/> index by string.
/// </summary>
/// <typeparam name="TValue">The type indexed by string.</typeparam>
public interface IStringMap<TValue> where TValue : class
{
    /// <summary>
    /// The number of key/ value pairs set in the map.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Find a value by it's corresponding key.
    /// </summary>
    /// <param name="key">The key of a value in the index.</param>
    /// <param name="value">A return value from the index.</param>
    /// <returns>Returns <c>true</c> if the key was found in the map.</returns>
    bool TryGetValue(string key, out TValue? value);
}
