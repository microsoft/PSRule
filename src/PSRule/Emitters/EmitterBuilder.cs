// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// A helper to build an <see cref="EmitterCollection"/>.
/// </summary>
internal sealed class EmitterBuilder
{
    private static readonly EventId PSR0012 = new(12, "PSR0012");
    private static readonly EventId PSR0013 = new(13, "PSR0013");

    private readonly ILanguageScopeSet? _LanguageScopeSet;
    private readonly IFormatOption _FormatOption;
    private readonly string? _StringFormat;
    private readonly List<KeyValuePair<string, Type>> _EmitterTypes;
    private readonly ServiceCollection _Services;
    private readonly ILogger? _Logger;

    /// <summary>
    /// Allow some emitters to always be enabled.
    /// This is a special case for testing, that's not available for general use.
    /// </summary>
    private readonly bool _AllowAlwaysEnabled;

    public EmitterBuilder(ILanguageScopeSet? languageScopeSet = default, IFormatOption? formatOption = default, string? stringFormat = default, ILogger? logger = default, bool allowAlwaysEnabled = false)
    {
        _LanguageScopeSet = languageScopeSet;
        _FormatOption = formatOption ?? new FormatOption();
        _StringFormat = stringFormat;
        _EmitterTypes = [];
        _Services = new ServiceCollection();
        _Logger = logger;
        _AllowAlwaysEnabled = allowAlwaysEnabled;
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
        if (!IsEmitterEnabled(typeof(T))) return;

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, typeof(T)));
        LogAddedEmitterDiagnostic(typeof(T).FullName, scope);
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
        if (!typeof(IEmitter).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
        if (!IsEmitterEnabled(type)) return;

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, type));
        LogAddedEmitterDiagnostic(type.FullName, scope);
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
        if (!IsEmitterEnabled(instance.GetType())) return;

        _EmitterTypes.Add(new KeyValuePair<string, Type>(scope, typeof(T)));
        LogAddedEmitterDiagnostic(typeof(T).FullName, scope);
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

        LogCompletedChainDiagnostic(emitters.Count);
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

    /// <summary>
    /// Determine if the specific format is configured as enabled.
    /// </summary>
    private bool IsFormatEnabled(string? format)
    {
        return format != null && !string.IsNullOrEmpty(format) && _FormatOption != null &&
            _FormatOption.TryGetValue(format, out var value) && value != null && value.Enabled == true;
    }

    /// <summary>
    /// Determine if the specified format is the format for handling string input.
    /// </summary>
    private bool IsStringFormat(string? format)
    {
        return format != null && _StringFormat != null && string.Equals(_StringFormat, format, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determine if the format is always enabled.
    /// </summary>
    private bool IsAlwaysEnabled(string? format)
    {
        return _AllowAlwaysEnabled && format != null && format == "*";
    }

    /// <summary>
    /// Determine is the emitter is configured as enabled.
    /// </summary>
    private bool IsEmitterEnabled(Type type)
    {
        var format = type.GetCustomAttribute<EmitterFormatAttribute>(inherit: false)?.Format;
        return IsFormatEnabled(format) || IsStringFormat(format) || IsAlwaysEnabled(format);
    }

    /// <summary>
    /// PSR0012: Added emitter '{0}' to scope '{1}'.
    /// </summary>
    private void LogAddedEmitterDiagnostic(string type, string scope)
    {
        if (_Logger == null)
            return;

        _Logger.LogDebug(PSR0012, PSRuleResources.PSR0012, type, scope);
    }

    /// <summary>
    /// PSR0013: Completed building chain using '{0}' emitters.
    /// </summary>
    private void LogCompletedChainDiagnostic(int count)
    {
        if (_Logger == null)
            return;

        _Logger.LogDebug(PSR0013, PSRuleResources.PSR0013, count);
    }
}

#nullable restore
