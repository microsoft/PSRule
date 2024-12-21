// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// A helper to build an <see cref="EmitterCollection"/>.
/// </summary>
internal sealed class EmitterBuilder
{
    private readonly ILanguageScopeSet? _LanguageScopeSet;
    private readonly IFormatOption _FormatOption;
    private readonly List<KeyValuePair<string, Type>> _EmitterTypes;
    private readonly ServiceCollection _Services;

    public EmitterBuilder(ILanguageScopeSet? languageScopeSet = default, IFormatOption? formatOption = default)
    {
        _LanguageScopeSet = languageScopeSet;
        _FormatOption = formatOption ?? new FormatOption();
        _EmitterTypes = new List<KeyValuePair<string, Type>>(4);
        _Services = new ServiceCollection();
        AddInternalServices();
        AddInternalEmitters();
        AddEmittersFromLanguageScope();
    }

    /// <summary>
    /// Add an <see cref="IEmitter"/> implementation class.
    /// </summary>
    /// <param name="scope">The scope of the emitter.</param>
    /// <typeparam name="T">An emitter type that implements <see cref="IEmitter"/>.</typeparam>
    /// <exception cref="ArgumentNullException">The <paramref name="scope"/> parameter must not be a null or empty string.</exception>
    public void AddEmitter<T>(string scope) where T : class, IEmitter
    {
        if (string.IsNullOrEmpty(scope)) throw new ArgumentNullException(nameof(scope));

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, typeof(T)));
        _Services.AddScoped(typeof(T));
    }

    /// <summary>
    /// Add an <see cref="IEmitter"/> implementation class.
    /// </summary>
    /// <param name="scope">The scope of the emitter.</param>
    /// <param name="type">An emitter type that implements <see cref="IEmitter"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="scope"/> parameter must not be a null or empty string.</exception>
    public void AddEmitter(string scope, Type type)
    {
        if (string.IsNullOrEmpty(scope)) throw new ArgumentNullException(nameof(scope));

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, type));
        _Services.AddScoped(type);
    }

    /// <summary>
    /// Add an existing emitter instance that is already configured.
    /// </summary>
    /// <typeparam name="T">An emitter type that implements <see cref="IEmitter"/>.</typeparam>
    /// <param name="scope">The scope of the emitter.</param>
    /// <param name="instance">The specific instance.</param>
    /// <exception cref="ArgumentNullException">Both the <paramref name="scope"/> and <paramref name="instance"/> parameters must not be null or empty string.</exception>
    public void AddEmitter<T>(string scope, T instance) where T : class, IEmitter
    {
        if (string.IsNullOrEmpty(scope)) throw new ArgumentNullException(nameof(scope));
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, typeof(T)));
        _Services.AddScoped(services =>
        {
            return instance;
        });
    }

    /// <summary>
    /// Build a collection of <see cref="IEmitter"/> that can be called as a group.
    /// </summary>
    /// <param name="context">A context object for the collection.</param>
    /// <returns>An instance of <see cref="EmitterCollection"/>.</returns>
    public EmitterCollection Build(IEmitterContext context)
    {
        var currentScopeName = string.Empty;

        _Services.AddScoped(provider => GetScopedConfiguration(currentScopeName, provider));

        var serviceProvider = _Services.BuildServiceProvider();
        var emitters = new List<IEmitter>(_EmitterTypes.Count);
        var scopes = new List<IServiceScope>();

        foreach (var group in _EmitterTypes.GroupBy(kv => kv.Key))
        {
            currentScopeName = group.Key;
            var scope = serviceProvider.CreateScope();

            foreach (var type in group)
            {
                if (scope.ServiceProvider.GetRequiredService(type.Value) is IEmitter emitter)
                {
                    emitters.Add(emitter);
                }
            }

            scopes.Add(scope);
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
        AddEmitter<YamlEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);
        AddEmitter<JsonEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);
        AddEmitter<MarkdownEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);
        AddEmitter<PowerShellDataEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);
    }

    /// <summary>
    /// Add custom emitters from the language scope.
    /// </summary>
    private void AddEmittersFromLanguageScope()
    {
        if (_LanguageScopeSet == null) return;

        foreach (var scope in _LanguageScopeSet.Get())
        {
            foreach (var emitterType in scope.GetEmitters())
            {
                AddEmitter(scope.Name, emitterType);
            }
        }
    }

    /// <summary>
    /// Create a configuration for the emitter based on it's scope.
    /// </summary>
    private IEmitterConfiguration GetScopedConfiguration(string scope, IServiceProvider serviceProvider)
    {
        if (_LanguageScopeSet == null ||
            !_LanguageScopeSet.TryScope(scope, out var languageScope) ||
            languageScope == null)
            return EmptyEmitterConfiguration.Instance;

        var configuration = languageScope.ToConfiguration();
        return new InternalEmitterConfiguration(configuration, _FormatOption);
    }
}

#nullable restore
