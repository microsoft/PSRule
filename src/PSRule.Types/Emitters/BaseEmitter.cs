// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Emitters;

/// <summary>
/// A base class for implementing an emitter.
/// </summary>
public abstract class BaseEmitter : IEmitter
{
    private bool _Disposed;

    /// <inheritdoc/>
    public abstract bool Visit(IEmitterContext context, object o);

    /// <inheritdoc/>
    public abstract bool Accepts(IEmitterContext context, Type type);

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">Determines if a dispose is occurring.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                // Do nothing here.
            }
            _Disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
