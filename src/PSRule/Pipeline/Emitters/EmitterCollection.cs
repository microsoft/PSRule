// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Microsoft.Extensions.DependencyInjection;
using PSRule.Data;
using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters;

#nullable enable

/// <summary>
/// A collection of <seealso cref="IEmitter"/> instances.
/// </summary>
internal sealed class EmitterCollection : IDisposable
{
    private readonly ServiceProvider _ServiceProvider;

    private readonly IEmitterContext _Context;
    private readonly EmitterChain _Chain;

    /// <summary>
    /// Determines if file are emitted for processing.
    /// This is for backwards compatibility and will be removed for v4.
    /// </summary>
    private readonly bool _ShouldEmitFile;

    private bool _Disposed;

    public EmitterCollection(ServiceProvider serviceProvider, IEmitter[] emitters, IEmitterContext context)
    {
        _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Emitters = emitters ?? throw new ArgumentNullException(nameof(emitters));
        _Context = context ?? throw new ArgumentNullException(nameof(context));

        _Chain = BuildChain(emitters);
        _ShouldEmitFile = context?.ShouldEmitFile ?? false;
    }

    /// <summary>
    /// The number of emitters registered in the collection.
    /// </summary>
    public int Count => Emitters.Length;

    /// <summary>
    /// The emitters in the collection.
    /// </summary>
    public IEmitter[] Emitters { get; }

    /// <summary>
    /// Visit an object with applicable emitters.
    /// </summary>
    /// <param name="o">The object to visit.</param>
    /// <returns>Returns <c>true</c> if the object was processed by any emitter.</returns>
    public bool Visit(object? o)
    {
        if (o == null) return false;

        if (TryGetFile(o, out var info, out var exists) && info != null)
        {
            if (!exists) return false;

            if (_ShouldEmitFile && _Context.ShouldQueue(info.Path))
            {
                // Emit the file.
                _Context.Emit(new TargetObject(new PSObject(o)));
            }
            return VisitFileInternal(info);
        }

        return GetBaseObject(o) is string s && _Context.Format != Options.InputFormat.None ? VisitString(s) : VisitUnexpanded(o);
    }

    private bool VisitString(string s)
    {
        return _Chain != null && _Chain(_Context, s, typeof(string));
    }

    private bool VisitFileInternal(InternalFileInfo o)
    {
        if (_Chain == null) return false;
        if (!_Context.ShouldQueue(o.Path)) return false;

        // Emit the file content.
        var type = o.GetType();
        return _Chain(_Context, o, type);
    }

    private bool VisitUnexpanded(object o)
    {
        var targetObject = o is PSObject pso ? pso : new PSObject(o);
        _Context.Emit(new TargetObject(targetObject));
        return true;
    }

    private static object GetBaseObject(object o)
    {
        return o is PSObject pso ? pso.BaseObject : o;
    }

    private static bool TryGetFile(object o, out InternalFileInfo? info, out bool exists)
    {
        info = null;
        exists = false;
        o = GetBaseObject(o);
        if (o is FileInfo fileInfo)
        {
            info = new InternalFileInfo(fileInfo.FullName, fileInfo.Extension);
            exists = fileInfo.Exists;
            return true;
        }
        if (o is InputFileInfo inputFileInfo)
        {
            info = new InternalFileInfo(inputFileInfo.FullName, inputFileInfo.Extension);
            exists = inputFileInfo.AsFileInfo().Exists;
            return true;
        }
        if (o is InternalFileInfo internalFile)
        {
            info = internalFile;
            exists = File.Exists(internalFile.Path);
            return true;
        }
        return false;
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                for (var i = 0; _Chain != null && i < Emitters.Length; i++)
                    Emitters[i].Dispose();

                _ServiceProvider.Dispose();
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

    /// <summary>
    /// Build a chain of emitters.
    /// </summary>
    /// <param name="emitters">The emitters to include in the chain.</param>
    /// <returns>A chain of emitters.</returns>
    private static EmitterChain BuildChain(IEmitter[] emitters)
    {
        if (emitters == null || emitters.Length == 0)
            throw new ArgumentNullException(nameof(emitters));

        // Get emitters in reverse order.
        var result = Create(emitters[emitters.Length - 1], null);
        for (var i = emitters.Length - 2; i >= 0; i--)
            result = Create(emitters[i], result);

        return result;
    }

    private static EmitterChain Create(IEmitter e, EmitterChain? next)
    {
        return next == null ?
            (context, o, type) => e.Accepts(context, type) && e.Visit(context, o) :
            (context, o, type) =>
            {
                var r1 = e.Accepts(context, type) && e.Visit(context, o);
                var r2 = next(context, o, type);
                return r1 || r2;
            };
    }
}

#nullable restore
