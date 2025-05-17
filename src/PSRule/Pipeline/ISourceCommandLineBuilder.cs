// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a list of sources for a command-line tool pipeline.
/// </summary>
public interface ISourceCommandLineBuilder
{
    /// <summary>
    /// Add loose files as a source.
    /// </summary>
    /// <param name="path">An array of file or directory paths containing one or more rule files.</param>
    /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
    void Directory(string[] path, bool excludeDefaultRulePath = false);

    /// <summary>
    /// Add loose files as a source.
    /// </summary>
    /// <param name="path">A file or directory path containing one or more rule files.</param>
    /// <param name="excludeDefaultRulePath">Determine if the default rule path is excluded. When set to <c>true</c> the default rule path is excluded.</param>
    void Directory(string path, bool excludeDefaultRulePath = false);

    /// <summary>
    /// Add a module source.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    /// <param name="version">A specific version of the module.</param>
    void ModuleByName(string name, string? version = null);

    /// <summary>
    /// Build a list of sources for executing within PSRule.
    /// </summary>
    /// <returns>A list of sources.</returns>
    Source[] Build();
}
