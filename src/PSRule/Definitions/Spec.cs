// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// The base class for a resource specification.
/// </summary>
public abstract class Spec
{
    private const string FullNameSeparator = "/";

    /// <summary>
    /// Create an instance of the resource specification.
    /// </summary>
    protected Spec() { }

    /// <summary>
    /// Get a fully qualified name for the resource type.
    /// </summary>
    /// <param name="apiVersion">The specific API version of the resource.</param>
    /// <param name="name">The type name of the resource.</param>
    /// <returns>A fully qualified type name string.</returns>
    public static string GetFullName(string apiVersion, string name)
    {
        return string.Concat(apiVersion, FullNameSeparator, name);
    }
}
