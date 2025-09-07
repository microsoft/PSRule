// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Collections;
using PSRule.Rules;

namespace PSRule.Definitions.Rules;

internal sealed class RuleV1ScriptSpec : Spec, IDisposable
{
    private bool _Disposed;

    public PowerShellCondition Condition { get; internal set; }
    public SeverityLevel Level { get; internal set; }
    public ResourceId[]? DependsOn { get; internal set; }
    public string[]? Type { get; internal set; }
    public string[]? With { get; internal set; }
    public Hashtable? Configure { get; internal set; }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                Condition?.Dispose();
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
}
