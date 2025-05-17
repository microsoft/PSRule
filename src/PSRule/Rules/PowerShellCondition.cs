// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Management.Automation;
using PSRule.Definitions;

namespace PSRule.Rules;

internal sealed class PowerShellCondition : ICondition
{
    private const string ERROR_ACTION_PREFERENCE = "ErrorActionPreference";

    private readonly PowerShell _Condition;

    private bool _Disposed;

    internal PowerShellCondition(ResourceId id, ISourceFile source, PowerShell condition, ActionPreference errorAction)
    {
        _Condition = condition;
        Id = id;
        Source = source;
        ErrorAction = errorAction;
    }

    public ResourceId Id { get; }

    public ISourceFile Source { get; }

    public ActionPreference ErrorAction { get; }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Condition.Runspace = null;
                _Condition.Dispose();
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public IConditionResult If()
    {
        _Condition.Streams.ClearStreams();
        _Condition.Runspace.SessionStateProxy.SetVariable(ERROR_ACTION_PREFERENCE, ErrorAction);
        return GetResult(_Condition.Invoke<Runtime.RuleConditionResult>());
    }

    private static Runtime.RuleConditionResult? GetResult(Collection<Runtime.RuleConditionResult> value)
    {
        return value == null || value.Count == 0 ? null : value[0];
    }
}
