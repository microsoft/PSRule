// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A source file containing resources that will be loaded and interpreted by PSRule.
/// </summary>
public interface ISourceFile
{
    /// <summary>
    /// The file path to the source.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// The name of the module if the source was loaded from a module.
    /// </summary>
    string? Module { get; }

    /// <summary>
    /// The type of source file.
    /// </summary>
    SourceType Type { get; }

    /// <summary>
    /// The base path to use for loading help content.
    /// </summary>
    string HelpPath { get; }

    /// <summary>
    /// Determines if the source file exists.
    /// </summary>
    /// <returns>Returns <c>true</c> when the source file exists.</returns>
    bool Exists();

    /// <summary>
    /// Determines if the source file is a dependency.
    /// </summary>
    /// <returns>Returns <c>true</c> when the source file is a dependency.</returns>
    bool IsDependency();
}
