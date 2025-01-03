// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// The internal implementation of a <see cref="IEmitterContext"/>.
/// </summary>
internal sealed class EmitterContext : BaseEmitterContext
{
    private readonly ConcurrentQueue<ITargetObject> _Queue;
    private readonly PathFilter? _InputFilter;

    /// <summary>
    /// Create an instance containing context for an <see cref="IEmitter"/>.
    /// </summary>
    internal EmitterContext(ConcurrentQueue<ITargetObject> queue, PathFilter? inputFilter, PSRuleOption? option)
        : base(option?.Input?.Format ?? InputFormat.None, option?.Input?.ObjectPath, option?.Input?.FileObjects ?? false)
    {
        _Queue = queue;
        _InputFilter = inputFilter;
    }

    /// <inheritdoc/>
    protected override void Enqueue(ITargetObject value)
    {
        if (!ShouldQueue(value))
            return;

        _Queue.Enqueue(value);
    }

    /// <summary>
    /// Avoid queuing objects with an excluded source.
    /// </summary>
    private bool ShouldQueue(ITargetObject targetObject)
    {
        if (_InputFilter == null)
            return true;

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

#nullable restore
