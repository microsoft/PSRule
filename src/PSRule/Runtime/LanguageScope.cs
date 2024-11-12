// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Runtime.Binding;

namespace PSRule.Runtime;

#nullable enable

[DebuggerDisplay("{Name}")]
internal sealed class LanguageScope : ILanguageScope
{
    private IDictionary<string, object>? _Configuration;
    private readonly Dictionary<string, object> _Service;
    private readonly Dictionary<ResourceKind, IResourceFilter> _Filter;
    private ITargetBinder? _TargetBinder;
    private StringComparer? _BindingComparer;

    private bool _Disposed;

    public LanguageScope(string name)
    {
        _Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        Name = ResourceHelper.NormalizeScope(name);
        _Filter = [];
        _Service = [];
    }

    /// <inheritdoc/>
    public string Name { [DebuggerStepThrough] get; }

    /// <inheritdoc/>
    public string[]? Culture { [DebuggerStepThrough] get; [DebuggerStepThrough] private set; }

    public StringComparer GetBindingComparer() => _BindingComparer ?? StringComparer.OrdinalIgnoreCase;

    /// <inheritdoc/>
    public void Configure(Dictionary<string, object> configuration)
    {
        _Configuration ??= new Dictionary<string, object>();
        _Configuration.AddUnique(configuration);
    }

    /// <inheritdoc/>
    public void Configure(OptionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        _Configuration = context.Configuration;
        _Configuration ??= new Dictionary<string, object>();
        WithFilter(context.RuleFilter);
        WithFilter(context.ConventionFilter);
        _BindingComparer = context.Binding.GetComparer();
        Culture = context.Output.Culture;

        var builder = new TargetBinderBuilder(context.BindTargetName, context.BindTargetType, context.BindField, context.InputTargetType);
        _TargetBinder = builder.Build(context.Binding);
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string key, out object? value)
    {
        value = default;
        return !string.IsNullOrEmpty(key) && _Configuration != null && _Configuration.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public void WithFilter(IResourceFilter resourceFilter)
    {
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
        if (_Service.ContainsKey(name))
            return;

        _Service.Add(name, service);
    }

    /// <inheritdoc/>
    public object? GetService(string name)
    {
        return _Service.TryGetValue(name, out var service) ? service : null;
    }

    public ITargetBindingResult? Bind(TargetObject targetObject)
    {
        return _TargetBinder?.Bind(targetObject);
    }

    public ITargetBindingResult? Bind(object targetObject)
    {
        return _TargetBinder?.Bind(targetObject);
    }

    /// <inheritdoc/>
    public bool TryGetType(object o, out string? type, out string? path)
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
    public bool TryGetName(object o, out string? name, out string? path)
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
}

#nullable restore
