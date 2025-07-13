// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Emitters;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Runtime.Binding;

namespace PSRule.Runtime;

[DebuggerDisplay("{Name}")]
internal sealed class LanguageScope(string name, RuntimeFactoryContainer? container) : ILanguageScope, IRuntimeServiceCollection
{
    private readonly RuntimeFactoryContainer? _Container = container;
    private readonly Dictionary<string, object> _Service = [];
    private readonly List<Type> _EmitterTypes = [];
    private readonly Dictionary<ResourceKind, IResourceFilter> _Filter = [];

    private IDictionary<string, object>? _Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    private WildcardMap<RuleOverride>? _Override;
    private ITargetBinder? _TargetBinder;
    private StringComparer? _BindingComparer;

    private bool _Disposed;
    private bool _RunOnce;

    /// <inheritdoc/>
    public string Name { [DebuggerStepThrough] get; } = ResourceHelper.NormalizeScope(name);

    /// <inheritdoc/>
    public string[]? Culture { [DebuggerStepThrough] get; [DebuggerStepThrough] private set; }

    public StringComparer GetBindingComparer() => _BindingComparer ?? StringComparer.OrdinalIgnoreCase;

    /// <inheritdoc/>
    public void Configure(OptionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        _Configuration = context.Configuration;
        _Configuration ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        WithFilter(context.RuleFilter);
        WithFilter(context.ConventionFilter);
        _BindingComparer = context.Binding.GetComparer();
        Culture = context.Output?.Culture;

        var builder = new TargetBinderBuilder(context.BindTargetName, context.BindTargetType, context.BindField, context.InputTargetType);
        _TargetBinder = builder.Build(context.Binding);
        _Override = WithOverride(context.Override);

        if (_Container != null && !_RunOnce)
        {
            _RunOnce = true;
            _Container.Configure(new RuntimeFactoryContext(this));
        }
    }

    private static WildcardMap<RuleOverride>? WithOverride(OverrideOption? option)
    {
        if (option == null || option.Level == null)
            return default;

        var overrides = option.Level
            .Where(l => l.Value != SeverityLevel.None)
            .Select(l => new KeyValuePair<string, RuleOverride>(l.Key, new RuleOverride { Level = l.Value }));

        return new WildcardMap<RuleOverride>(overrides);
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string key, out object? value)
    {
        value = default;
        return !string.IsNullOrEmpty(key) && _Configuration != null && _Configuration.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public bool TryGetOverride(ResourceId id, out RuleOverride? value)
    {
        value = default;
        if (_Override == null) return false;

        return _Override.TryGetValue(id.Value, out value) ||
            _Override.TryGetValue(id.Name, out value);
    }

    /// <inheritdoc/>
    public void WithFilter(IResourceFilter? resourceFilter)
    {
        if (resourceFilter == null)
            return;

        _Filter[resourceFilter.Kind] = resourceFilter;
    }

    /// <inheritdoc/>
    public IResourceFilter? GetFilter(ResourceKind kind)
    {
        return _Filter.TryGetValue(kind, out var filter) ? filter : null;
    }

    /// <inheritdoc/>
    public void AddService(string name, object service)
    {
        if (string.IsNullOrEmpty(name) || service == null || _Service.ContainsKey(name))
            return;

        _Service.Add(name, service);
    }

    /// <summary>
    /// Configure services to the scope.
    /// </summary>
    /// <param name="configure">An delegate that configures zero or many services in the current scope.</param>
    public void ConfigureServices(Action<IRuntimeServiceCollection>? configure)
    {
        if (configure == null)
            return;

        // Configure services
        configure(this);
    }

    /// <inheritdoc/>
    public object? GetService(string name)
    {
        return _Service.TryGetValue(name, out var service) ? service : null;
    }

    /// <inheritdoc/>
    public IEnumerable<Type> GetEmitters()
    {
        return _EmitterTypes;
    }

    public ITargetBindingResult? Bind(ITargetObject targetObject)
    {
        return _TargetBinder?.Bind(targetObject);
    }

    /// <inheritdoc/>
    public bool TryGetType(ITargetObject o, out string? type, out string? path)
    {
        if (_TargetBinder != null)
        {
            var result = _TargetBinder.Bind(o);
            type = result.TargetType;
            path = result.TargetTypePath;
            return true;
        }
        type = default;
        path = default;
        return false;
    }

    /// <inheritdoc/>
    public bool TryGetName(ITargetObject o, out string? name, out string? path)
    {
        if (_TargetBinder != null)
        {
            var result = _TargetBinder.Bind(o);
            name = result.TargetName;
            path = result.TargetNamePath;
            return true;
        }
        name = default;
        path = default;
        return false;
    }

    public IConfiguration ToConfiguration()
    {
        return new InternalConfiguration(_Configuration ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                // Release and dispose services
                if (_Service != null && _Service.Count > 0)
                {
                    foreach (var kv in _Service)
                    {
                        if (kv.Value is IDisposable d)
                            d.Dispose();
                    }
                    _Service.Clear();
                }
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

    #region IRuntimeServiceCollection

    /// <inheritdoc/>
    string IRuntimeServiceCollection.ScopeName => Name;

    /// <inheritdoc/>
    IConfiguration IRuntimeServiceCollection.Configuration => ToConfiguration();

    /// <inheritdoc/>
    void IRuntimeServiceCollection.AddService<TInterface, TService>()
    {
        // Add any emitter.
        if (typeof(TInterface) == typeof(IEmitter))
            _EmitterTypes.Add(typeof(TService));
    }

    #endregion IRuntimeServiceCollection
}
