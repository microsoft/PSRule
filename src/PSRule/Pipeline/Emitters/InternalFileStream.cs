// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Data;

namespace PSRule.Pipeline.Emitters;

internal sealed class InternalFileStream : IFileStream
{
    private readonly Stream _Stream;

    private bool _Disposed;

    internal InternalFileStream(IFileInfo info, Stream stream)
    {
        Info = info;
        _Stream = stream;
    }

    /// <inheritdoc/>
    public IFileInfo Info { get; }

    /// <inheritdoc/>
    public TextReader AsTextReader()
    {
        return new StreamReader(
            stream: _Stream,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true
        );
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Stream?.Dispose();
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
