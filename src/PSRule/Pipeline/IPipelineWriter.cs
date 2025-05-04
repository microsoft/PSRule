// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// An writer which receives output from PSRule.
/// </summary>
public interface IPipelineWriter : IDisposable, ILogger
{
    /// <summary>
    /// Get the last exit code.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// Determines if any errors were reported.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// Determines if any failures were reported.
    /// </summary>
    bool HadFailures { get; }

    /// <summary>
    /// Write a message to the host process.
    /// </summary>
    void WriteHost(HostInformationMessage info);

    /// <summary>
    /// Write an object to output.
    /// </summary>
    /// <param name="o">The object to write to output.</param>
    /// <param name="enumerateCollection">Determines when the object is enumerable if it should be enumerated as more then one object.</param>
    void WriteObject(object o, bool enumerateCollection);

    /// <summary>
    /// Write a result to the pipeline.
    /// </summary>
    void WriteResult(InvokeResult result);

    /// <summary>
    /// Start and initialize the writer.
    /// </summary>
    void Begin();

    /// <summary>
    /// Stop and finalized the writer.
    /// </summary>
    void End(IPipelineResult result);

    /// <summary>
    /// Set the terminating exit code of the pipeline.
    /// </summary>
    /// <param name="exitCode">The numerical exit code.</param>
    void SetExitCode(int exitCode);
}
