// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A property of a target object.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public interface ITargetObjectProperty<T>
{
    /// <summary>
    /// The value of the property.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// The object path to the property.
    /// </summary>
    string? Path { get; }
}
