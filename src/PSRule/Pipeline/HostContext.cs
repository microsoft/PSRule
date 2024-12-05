// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;

namespace PSRule.Pipeline;

#nullable enable

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

    /// <inheritdoc/>
    public virtual string? CachePath => null;
}

#nullable restore
