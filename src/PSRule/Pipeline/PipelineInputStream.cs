// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Emitters;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A stream of input objects that will be evaluated.
/// </summary>
internal sealed class PipelineInputStream : IPipelineReader
{
    private readonly InputPathBuilder? _InputPath;
    private readonly IPathFilter? _InputFilter;
    private readonly ConcurrentQueue<ITargetObject> _Queue;
    private readonly ILogger? _Logger;
    private readonly EmitterCollection _EmitterCollection;

    public PipelineInputStream(ILanguageScopeSet? languageScopeSet, InputPathBuilder? inputPath, IPathFilter? inputFilter, PSRuleOption? option, ILogger? logger)
    {
        _InputPath = inputPath;
        _InputFilter = inputFilter;
        _Queue = new ConcurrentQueue<ITargetObject>();
        _Logger = logger;
        _EmitterCollection = new EmitterBuilder(languageScopeSet, option?.Format, option?.Input?.StringFormat, logger).Build(new EmitterContext(_Queue, inputFilter, option));
    }

    public int Count => _Queue.Count;

    public bool IsEmpty => _Queue.IsEmpty;

    /// <inheritdoc/>
    public void Enqueue(object sourceObject, string? targetType = null, bool skipExpansion = false)
    {
        if (sourceObject == null)
            return;

        var targetObject = new TargetObject(sourceObject is PSObject pso ? pso : new PSObject(sourceObject), type: targetType);
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
        _Logger?.LogDebug(EventId.None, "Opening input stream.");
        if (_InputPath == null || _InputPath.Count == 0)
            return;

        // Read each file
        var files = _InputPath.Build();
        for (var i = 0; i < files.Length; i++)
        {
            _Logger?.LogDebug(EventId.None, "opening with: {0}", files[i].Path);
            EnqueueFile(files[i]);
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
        if (string.IsNullOrEmpty(path) || _InputPath == null)
            return;

        path = Environment.GetRootedPath(path, normalize: true);
        var basePath = Environment.GetRootedBasePath(null, normalize: true);

        _Logger?.Log(LogLevel.Debug, EventId.None, null, PSRuleResources.InputAdded, path);

        _InputPath.Add(path, useGlobalFilter: false);
    }

    private void EnqueueFile(InputFileInfo file)
    {
        if (file.IsUrl)
        {
            Enqueue(PSObject.AsPSObject(new Uri(file.FullName)));
        }
        else
        {
            Enqueue(file);
        }
    }
}

#nullable restore
