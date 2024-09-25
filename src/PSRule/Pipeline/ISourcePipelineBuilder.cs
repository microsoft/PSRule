// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a list of sources for a PowerShell pipeline.
/// </summary>
public interface ISourcePipelineBuilder
{
    /// <summary>
    /// Determines if PowerShell should automatically load the module.
    /// </summary>
    bool ShouldLoadModule { get; }

    /// <summary>
    /// Log a verbose message for scanning sources.
    /// </summary>
    void VerboseScanSource(string path);

    /// <summary>
    /// Log a verbose message for source modules.
    /// </summary>
    void VerboseFoundModules(int count);

    /// <summary>
    /// Log a verbose message for scanning for modules.
    /// </summary>
    void VerboseScanModule(string moduleName);

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
    /// <param name="module">The module info.</param>
    void Module(PSModuleInfo[] module);

    /// <summary>
    /// Build a list of sources for executing within PSRule.
    /// </summary>
    /// <returns>A list of sources.</returns>
    Source[] Build();
}
