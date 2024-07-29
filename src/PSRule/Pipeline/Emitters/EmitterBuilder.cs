// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using PSRule.Emitters;
using PSRule.Runtime;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// A helper to build an <see cref="EmitterCollection"/>.
/// </summary>
internal sealed class EmitterBuilder
{
    private readonly List<Type> _EmitterTypes;
    private readonly ServiceCollection _Services;

    public EmitterBuilder()
    {
        _EmitterTypes = new List<Type>(4);
        _Services = new ServiceCollection();
        AddInternalServices();
        AddInternalEmitters();
    }

    /// <summary>
    /// Add an <see cref="IEmitter"/> implementation class.
    /// </summary>
    /// <typeparam name="T">An emitter type that implements <see cref="IEmitter"/>.</typeparam>
    public void AddEmitter<T>() where T : class, IEmitter
    {
        _EmitterTypes.Add(typeof(T));
        _Services.AddTransient(typeof(T));
    }

    /// <summary>
    /// Build a collection of <see cref="IEmitter"/> that can be called as a group.
    /// </summary>
    /// <param name="context">A context object for the collection.</param>
    /// <returns>An instance of <see cref="EmitterCollection"/>.</returns>
    public EmitterCollection Build(IEmitterContext context)
    {
        var serviceProvider = _Services.BuildServiceProvider();
        var emitters = new List<IEmitter>(_EmitterTypes.Count);

        foreach (var type in _EmitterTypes)
        {
            if (serviceProvider.GetRequiredService(type) is IEmitter emitter)
            {
                emitters.Add(emitter);
            }
        }

        return new EmitterCollection(serviceProvider, [.. emitters], context);
    }

    /// <summary>
    /// Add the default services automatically added to the DI container.
    /// </summary>
    private void AddInternalServices()
    {
        _Services.AddSingleton<ILoggerFactory, LoggerFactory>();
        _Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    /// <summary>
    /// Add the built-in emitters to the list of emitters for processing items.
    /// </summary>
    private void AddInternalEmitters()
    {
        AddEmitter<YamlEmitter>();
        AddEmitter<JsonEmitter>();
        AddEmitter<MarkdownEmitter>();
        AddEmitter<PowerShellDataEmitter>();
    }
}
