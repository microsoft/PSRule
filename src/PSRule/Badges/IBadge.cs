// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Badges;

/// <summary>
/// An instance of a badge created by the badge API.
/// </summary>
public interface IBadge
{
    /// <summary>
    /// Get the badge as SVG text content.
    /// </summary>
    string ToSvg();

    /// <summary>
    /// Write the SVG badge content directly to disk.
    /// </summary>
    void ToFile(string path);
}
