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
    /// Determines if any errors were reported.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// Determines if any failures were reported.
    /// </summary>
    bool HadFailures { get; }

    /// <summary>
    /// Write a verbose message.
    /// </summary>
    void WriteVerbose(string message);

    /// <summary>
    /// Determines if a verbose message should be written to output.
    /// </summary>
    bool ShouldWriteVerbose();

    /// <summary>
    /// Write a warning message.
    /// </summary>
    void WriteWarning(string message);

    /// <summary>
    /// Determines if a warning message should be written to output.
    /// </summary>
    bool ShouldWriteWarning();

    /// <summary>
    /// Write an error message.
    /// </summary>
    void WriteError(ErrorRecord errorRecord);

    /// <summary>
    /// Determines if an error message should be written to output.
    /// </summary>
    bool ShouldWriteError();

    /// <summary>
    /// Write an informational message.
    /// </summary>
    void WriteInformation(InformationRecord informationRecord);

    /// <summary>
    /// Write a message to the host process.
    /// </summary>
    void WriteHost(HostInformationMessage info);

    /// <summary>
    /// Determines if an informational message should be written to output.
    /// </summary>
    bool ShouldWriteInformation();

    /// <summary>
    /// Write a debug message.
    /// </summary>
    void WriteDebug(string text, params object[] args);

    /// <summary>
    /// Determines if a debug message should be written to output.
    /// </summary>
    bool ShouldWriteDebug();

    /// <summary>
    /// Write an object to output.
    /// </summary>
    /// <param name="sendToPipeline">The object to write to the pipeline.</param>
    /// <param name="enumerateCollection">Determines when the object is enumerable if it should be enumerated as more then one object.</param>
    void WriteObject(object sendToPipeline, bool enumerateCollection);

    /// <summary>
    /// Enter a logging scope.
    /// </summary>
    void EnterScope(string scopeName);

    /// <summary>
    /// Exit a logging scope.
    /// </summary>
    void ExitScope();

    /// <summary>
    /// Start and initialize the writer.
    /// </summary>
    void Begin();

    /// <summary>
    /// Stop and finalized the writer.
    /// </summary>
    void End(IPipelineResult result);
}
