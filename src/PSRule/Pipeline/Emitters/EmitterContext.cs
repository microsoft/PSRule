// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using PSRule.Data;
using PSRule.Emitters;
using PSRule.Options;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// The internal implementation of a <see cref="IEmitterContext"/>.
/// </summary>
internal sealed class EmitterContext : BaseEmitterContext
{
    private readonly ConcurrentQueue<ITargetObject> _Queue;
    private readonly PathFilter _InputFilter;

    /// <summary>
    /// Create an instance containing context for an <see cref="IEmitter"/>.
    /// </summary>
    internal EmitterContext(ConcurrentQueue<ITargetObject> queue, PathFilter inputFilter, InputFormat? inputFormat, string objectPath, bool? shouldEmitFile)
        : base(inputFormat ?? InputFormat.None, objectPath, shouldEmitFile ?? false)
    {
        _Queue = queue;
        _InputFilter = inputFilter;
    }

    /// <inheritdoc/>
    protected override void Enqueue(ITargetObject value)
    {
        if (!ShouldQueue(value)) return;

        _Queue.Enqueue(value);
    }

    /// <summary>
    /// Avoid queuing objects with an excluded source.
    /// </summary>
    private bool ShouldQueue(ITargetObject targetObject)
    {
        if (_InputFilter == null) return true;

        foreach (var source in targetObject.Source)
        {
            if (!_InputFilter.Match(source.File))
                return false;
        }
        return true;
    }

    public override bool ShouldQueue(string path)
    {
        return _InputFilter == null || _InputFilter.Match(path);
    }
}
