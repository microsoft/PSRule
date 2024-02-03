// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;

namespace PSRule.Pipeline;

/// <summary>
/// A host context for handling input and output emitted from the pipeline.
/// </summary>
public interface IHostContext
{
    /// <summary>
    /// Determines if the pipeline is executing in a remote PowerShell session.
    /// </summary>
    bool InSession { get; }

    /// <summary>
    /// Determines if the pipeline encountered any errors.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// Get the value of a PowerShell preference variable.
    /// </summary>
    ActionPreference GetPreferenceVariable(string variableName);

    /// <summary>
    /// Get the value of a named variable.
    /// </summary>
    T GetVariable<T>(string variableName);

    /// <summary>
    /// Set the value of a named variable.
    /// </summary>
    void SetVariable<T>(string variableName, T value);

    /// <summary>
    /// Handle an error reported by the pipeline.
    /// </summary>
    void Error(ErrorRecord errorRecord);

    /// <summary>
    /// Handle a warning reported by the pipeline.
    /// </summary>
    void Warning(string text);

    /// <summary>
    /// Handle an informational record reported by the pipeline.
    /// </summary>
    void Information(InformationRecord informationRecord);

    /// <summary>
    /// Handle a verbose message reported by the pipeline.
    /// </summary>
    void Verbose(string text);

    /// <summary>
    /// Handle a debug message reported by the pipeline.
    /// </summary>
    void Debug(string text);

    /// <summary>
    /// Handle an object emitted from the pipeline.
    /// </summary>
    void Object(object sendToPipeline, bool enumerateCollection);

    /// <summary>
    /// Determines if a destructive action such as overwriting a file should be processed.
    /// </summary>
    bool ShouldProcess(string target, string action);

    /// <summary>
    /// Get the current working path.
    /// </summary>
    string GetWorkingPath();
}

internal static class HostContextExtensions
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string InformationPreference = "InformationPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string DebugPreference = "DebugPreference";
    private const string AutoLoadingPreference = "PSModuleAutoLoadingPreference";

    public static ActionPreference GetErrorPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(ErrorPreference);
    }

    public static ActionPreference GetWarningPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(WarningPreference);
    }

    public static ActionPreference GetInformationPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(InformationPreference);
    }

    public static ActionPreference GetVerbosePreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(VerbosePreference);
    }

    public static ActionPreference GetDebugPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(DebugPreference);
    }

    public static PSModuleAutoLoadingPreference GetAutoLoadingPreference(this IHostContext hostContext)
    {
        return hostContext.GetVariable<PSModuleAutoLoadingPreference>(AutoLoadingPreference);
    }
}

/// <summary>
/// A base class for custom host context instances.
/// </summary>
public abstract class HostContext : IHostContext
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";

    /// <inheritdoc/>
    public virtual bool InSession => false;

    /// <inheritdoc/>
    public virtual bool HadErrors { get; protected set; }

    /// <inheritdoc/>
    public virtual void Debug(string text)
    {

    }

    /// <inheritdoc/>
    public virtual void Error(ErrorRecord errorRecord)
    {
        HadErrors = true;
    }

    /// <inheritdoc/>
    public virtual ActionPreference GetPreferenceVariable(string variableName)
    {
        return variableName == ErrorPreference ||
            variableName == WarningPreference ? ActionPreference.Continue : ActionPreference.Ignore;
    }

    /// <inheritdoc/>
    public virtual T GetVariable<T>(string variableName)
    {
        return default;
    }

    /// <inheritdoc/>
    public virtual void Information(InformationRecord informationRecord)
    {

    }

    /// <inheritdoc/>
    public virtual void Object(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is IResultRecord record)
            Record(record);
        //else if (enumerateCollection)
        //    foreach (var item in record)
    }

    /// <inheritdoc/>
    public virtual void SetVariable<T>(string variableName, T value)
    {

    }

    /// <inheritdoc/>
    public abstract bool ShouldProcess(string target, string action);

    /// <inheritdoc/>
    public virtual void Verbose(string text)
    {

    }

    /// <inheritdoc/>
    public virtual void Warning(string text)
    {

    }

    /// <summary>
    /// Handle record objects emitted from the pipeline.
    /// </summary>
    public virtual void Record(IResultRecord record)
    {

    }

    /// <inheritdoc/>
    public virtual string GetWorkingPath()
    {
        return Directory.GetCurrentDirectory();
    }
}

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
