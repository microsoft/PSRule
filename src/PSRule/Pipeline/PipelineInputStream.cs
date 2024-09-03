// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline.Emitters;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A stream of input objects that will be evaluated.
/// </summary>
internal sealed class PipelineInputStream : IPipelineReader
{
    private readonly InputPathBuilder _InputPath;
    private readonly PathFilter _InputFilter;
    private readonly ConcurrentQueue<ITargetObject> _Queue;
    private readonly EmitterCollection _EmitterCollection;

    public PipelineInputStream(InputPathBuilder inputPath, PathFilter inputFilter, PSRuleOption option)
    {
        _InputPath = inputPath;
        _InputFilter = inputFilter;
        _Queue = new ConcurrentQueue<ITargetObject>();
        _EmitterCollection = new EmitterBuilder().Build(new EmitterContext(_Queue, inputFilter, option));
    }

    public int Count => _Queue.Count;

    public bool IsEmpty => _Queue.IsEmpty;

    /// <inheritdoc/>
    public void Enqueue(object sourceObject, string? targetType = null, bool skipExpansion = false)
    {
        if (sourceObject == null)
            return;

        var targetObject = new TargetObject(sourceObject is PSObject pso ? pso : new PSObject(sourceObject), targetType: targetType);
        if (skipExpansion)
        {
            EnqueueInternal(targetObject);
            return;
        }
        _EmitterCollection.Visit(sourceObject);
    }

    /// <inheritdoc/>
    public bool TryDequeue(out ITargetObject sourceObject)
    {
        return _Queue.TryDequeue(out sourceObject);
    }

    /// <inheritdoc/>
    public void Open()
    {
        if (_InputPath == null || _InputPath.Count == 0)
            return;

        // Read each file
        var files = _InputPath.Build();
        for (var i = 0; i < files.Length; i++)
        {
            if (files[i].IsUrl)
            {
                Enqueue(PSObject.AsPSObject(new Uri(files[i].FullName)));
            }
            else
            {
                Enqueue(files[i]);
            }
        }
    }

    private void EnqueueInternal(TargetObject targetObject)
    {
        if (ShouldQueue(targetObject))
            _Queue.Enqueue(targetObject);
    }

    private bool ShouldQueue(TargetObject targetObject)
    {
        if (_InputFilter != null && targetObject.Source != null && targetObject.Source.Count > 0)
        {
            foreach (var source in targetObject.Source.GetSourceInfo())
                if (!_InputFilter.Match(source.File))
                    return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public void Add(string path)
    {
        _InputPath.Add(path);
    }
}

#nullable restore
