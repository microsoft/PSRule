// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Rules;

/// <summary>
/// An URL link to reference information.
/// </summary>
public sealed class Link
{
    internal Link(string name, string uri)
    {
        Name = name;
        Uri = uri;
    }

    /// <summary>
    /// The display name of the link.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The URL to the information, or the target link.
    /// </summary>
    public string Uri { get; }
}
