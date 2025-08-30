// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Expressions;
using PSRule.Runtime;

namespace PSRule.Rules;

/// <summary>
/// Define a condition implemented as a PowerShell script block.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
internal sealed class PowerShellCondition(ResourceId id, ISourceFile source, PowerShell condition, ActionPreference errorAction) : ICondition
{
    private const string ERROR_ACTION_PREFERENCE = "ErrorActionPreference";

    private readonly PowerShell _Condition = condition;

    private bool _Disposed;

    public ResourceId Id { get; } = id;

    public ISourceFile Source { get; } = source;

    public ActionPreference ErrorAction { get; } = errorAction;

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

    public IConditionResult? If(IExpressionContext expressionContext, ITargetObject o)
    {
        var context = new ExpressionContext(expressionContext, Source, ResourceKind.Rule, o);

        _Condition.Streams.ClearStreams();
        _Condition.Runspace.SessionStateProxy.SetVariable(ERROR_ACTION_PREFERENCE, ErrorAction);
        return GetResult(_Condition.Invoke<RuleConditionResult>());
    }

    private static RuleConditionResult? GetResult(Collection<RuleConditionResult> value)
    {
        return value == null || value.Count == 0 ? null : value[0];
    }
}
