// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// A helper to build an <see cref="EmitterCollection"/>.
/// </summary>
internal sealed class EmitterBuilder
{
    private readonly List<IEmitter> _Emitters;

    public EmitterBuilder()
    {
        _Emitters = new List<IEmitter>(3);
        AddInternal();
    }

    private void AddInternal()
    {
        Add<YamlEmitter>();
        Add<JsonEmitter>();
        Add<MarkdownEmitter>();
        Add<PowerShellDataEmitter>();
    }

    public void Add(IEmitter emitter)
    {
        _Emitters.Add(emitter);
    }

    public void Add<T>() where T : IEmitter, new()
    {
        Add((IEmitter)Activator.CreateInstance(typeof(T)));
    }

    /// <summary>
    /// Build a collection of <see cref="IEmitter"/> that can be called as a group.
    /// </summary>
    /// <param name="context">A context object for the collection.</param>
    /// <returns>An instance of <see cref="EmitterCollection"/>.</returns>
    public EmitterCollection Build(IEmitterContext context)
    {
        return new EmitterCollection(_Emitters.ToArray(), context);
    }
}
