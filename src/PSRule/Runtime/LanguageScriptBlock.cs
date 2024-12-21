// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;

namespace PSRule.Runtime;

internal sealed class LanguageScriptBlock(PowerShell block) : IDisposable
{
    private readonly PowerShell _Block = block;
    private readonly Stopwatch _Stopwatch = new();

    private bool _Disposed;

    /// <summary>
    /// The number of times the block was invoked.
    /// </summary>
    public int Count { get; private set; } = 0;

    /// <summary>
    /// The total number of milliseconds elapsed while invoking the block.
    /// </summary>
    public long Time => _Stopwatch.ElapsedMilliseconds;

    public void Invoke()
    {
        Count++;
        _Stopwatch.Start();
        try
        {
            _Block.Invoke();
        }
        finally
        {
            _Stopwatch.Stop();
        }
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Block.Runspace = null;
                _Block.Dispose();
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

    #endregion IDisposable
}
