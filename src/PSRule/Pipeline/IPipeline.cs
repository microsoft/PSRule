// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

/// <summary>
/// An instance of a PSRule pipeline.
/// </summary>
public interface IPipeline : IDisposable
{
    /// <summary>
    /// Get the pipeline result.
    /// </summary>
    IPipelineResult Result { get; }

    /// <summary>
    /// Initialize the pipeline and results. Call this method once prior to calling Process.
    /// </summary>
    void Begin();

    /// <summary>
    /// Process an object through the pipeline. Each object will be processed by rules that apply based on pre-conditions.
    /// </summary>
    /// <param name="sourceObject">The object to process.</param>
    void Process(PSObject sourceObject);

    /// <summary>
    /// Clean up and flush pipeline results. Call this method once after processing any objects through the pipeline.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches PowerShell pipeline.")]
    void End();
}
