// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Options;
using PSRule.Pipeline.Emitters;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A stream of input objects that will be evaluated.
/// </summary>
internal sealed class PipelineInputStream
{
    private readonly VisitTargetObject _Input;
    private readonly InputPathBuilder _InputPath;
    private readonly PathFilter _InputFilter;
    private readonly ConcurrentQueue<ITargetObject> _Queue;
    private readonly EmitterCollection _EmitterCollection;

    public PipelineInputStream(VisitTargetObject input, InputPathBuilder inputPath, PathFilter inputFilter, InputFormat? inputFormat, string objectPath, bool? shouldEmitFile)
    {
        _Input = input;
        _InputPath = inputPath;
        _InputFilter = inputFilter;
        _Queue = new ConcurrentQueue<ITargetObject>();
        _EmitterCollection = new EmitterBuilder().Build(new EmitterContext(_Queue, inputFilter, inputFormat, objectPath, shouldEmitFile));
    }

    public int Count => _Queue.Count;

    public bool IsEmpty => _Queue.IsEmpty;

    /// <summary>
    /// Add a new object into the stream.
    /// </summary>
    /// <param name="sourceObject">An object to process.</param>
    /// <param name="targetType">A pre-bound type.</param>
    /// <param name="skipExpansion">Determines if expansion is skipped.</param>
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

    public bool TryDequeue(out ITargetObject sourceObject)
    {
        return _Queue.TryDequeue(out sourceObject);
    }

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

    /// <summary>
    /// Add a path to the list of inputs.
    /// </summary>
    /// <param name="path">The path of files to add.</param>
    internal void Add(string path)
    {
        _InputPath.Add(path);
    }
}

#nullable restore
