// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;

namespace PSRule.Emitters;

[DebuggerDisplay("{Path}")]
internal sealed class InternalFileInfo : IFileInfo, IDisposable
{
    private FileStream _Stream;
    private bool _Disposed;

    public InternalFileInfo(string path, string extension)
    {
        Path = path;
        Extension = extension;
    }

    /// <inheritdoc/>
    public string Path { get; }

    /// <inheritdoc/>
    public string Extension { get; }

    /// <inheritdoc/>
    public IFileStream GetFileStream()
    {
        _Stream ??= File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _Stream.Position = 0;
        return new InternalFileStream(this, _Stream);
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Stream.Dispose();
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
