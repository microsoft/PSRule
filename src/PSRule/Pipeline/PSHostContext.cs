// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

/// <summary>
/// The host context used for PowerShell-based pipelines.
/// </summary>
public sealed class PSHostContext : IHostContext
{
    internal readonly PSCmdlet CmdletContext;
    internal readonly EngineIntrinsics ExecutionContext;

    /// <inheritdoc/>
    public bool InSession { get; }

    /// <inheritdoc/>
    public bool HadErrors { get; private set; }

    /// <summary>
    /// Create an instance of a PowerShell-based host context.
    /// </summary>
    public PSHostContext(PSCmdlet commandRuntime, EngineIntrinsics executionContext)
    {
        InSession = false;
        CmdletContext = commandRuntime;
        ExecutionContext = executionContext;
        InSession = executionContext != null && executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
    }

    /// <inheritdoc/>
    public ActionPreference GetPreferenceVariable(string variableName)
    {
        return ExecutionContext == null
            ? ActionPreference.SilentlyContinue
            : (ActionPreference)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
    }

    /// <inheritdoc/>
    public T GetVariable<T>(string variableName)
    {
        return ExecutionContext == null ? default : (T)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
    }

    /// <inheritdoc/>
    public void SetVariable<T>(string variableName, T value)
    {
        CmdletContext.SessionState.PSVariable.Set(variableName, value);
    }

    /// <inheritdoc/>
    public bool ShouldProcess(string target, string action)
    {
        return CmdletContext == null || CmdletContext.ShouldProcess(target, action);
    }

    /// <inheritdoc/>
    public void Error(ErrorRecord errorRecord)
    {
        CmdletContext.WriteError(errorRecord);
        HadErrors = true;
    }

    /// <inheritdoc/>
    public void Warning(string text)
    {
        CmdletContext.WriteWarning(text);
    }

    /// <inheritdoc/>
    public void Information(InformationRecord informationRecord)
    {
        CmdletContext.WriteInformation(informationRecord);
    }

    /// <inheritdoc/>
    public void Verbose(string text)
    {
        CmdletContext.WriteVerbose(text);
    }

    /// <inheritdoc/>
    public void Debug(string text)
    {
        CmdletContext.WriteDebug(text);
    }

    /// <inheritdoc/>
    public void Object(object sendToPipeline, bool enumerateCollection)
    {
        CmdletContext.WriteObject(sendToPipeline, enumerateCollection);
    }

    /// <inheritdoc/>
    public string GetWorkingPath()
    {
        return ExecutionContext.SessionState.Path.CurrentFileSystemLocation.Path;
    }
}
