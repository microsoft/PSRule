// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Management.Automation;

namespace PSRule.Runtime
{
    internal sealed class LanguageScriptBlock : IDisposable
    {
        private readonly PowerShell _Block;
        private bool disposedValue;

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
            if (!disposedValue)
            {
                if (disposing)
                {
                    _Block.Dispose();
                }
                disposedValue = true;
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
}
