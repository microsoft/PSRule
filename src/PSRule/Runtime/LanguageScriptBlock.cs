// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Runtime;

internal sealed class LanguageScriptBlock : IDisposable
{
    private readonly PowerShell _Block;

    private bool _Disposed;

    public LanguageScriptBlock(PowerShell block)
    {
        _Block = block;
    }

    public void Invoke()
    {
        _Block.Invoke();
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
