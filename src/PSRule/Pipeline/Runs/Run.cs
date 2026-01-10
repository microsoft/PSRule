// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Runtime;
using PSRule.Runtime.Binding;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// An instance of a run.
/// </summary>
[DebuggerDisplay("{Guid}: {Id}")]
internal sealed class Run(ILogger logger, string? scope, string id, InfoString description, string correlationGuid, IRuleGraph graph, RunConfiguration? configuration = null, ITargetBinder? targetBinder = null) : IRun
{
    private readonly ILogger _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly RunConfiguration? _RunConfiguration = configuration;
    private readonly ITargetBinder _TargetBinder = targetBinder ?? new TargetBinderBuilder(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, null).Build(null);

    private WildcardMap<RuleOverride>? _Override;

    /// <inheritdoc/>
    public string Id { get; } = id;

    /// <inheritdoc/>
    public string Guid { get; } = System.Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string CorrelationGuid { get; } = correlationGuid;

    /// <inheritdoc/>
    public InfoString Description { get; } = description;

    /// <summary>
    /// The results from the run.
    /// </summary>
    public InvokeResult? Result { get; set; }

    /// <summary>
    /// Get an ordered culture preference list which will be tries for finding help.
    /// </summary>
    public string[]? Culture { get; private set; }

    /// <inheritdoc/>
    public IRuleGraph Rules { get; } = graph;

    public string? Scope { get; } = scope;

    #region IConfiguration

    /// <inheritdoc/>
    public object? GetValueOrDefault(string configurationKey, object? defaultValue = null)
    {
        return TryConfigurationValue(configurationKey, out var value) ? value : defaultValue;
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string configurationKey, out object? value)
    {
        value = null;
        _Logger.LogDebug(EventId.None, "Run '{0}': Retrieving configuration key '{1}' from '{2}'", Guid, configurationKey, _RunConfiguration?.Guid);

        return _RunConfiguration != null && _RunConfiguration.Configuration.TryGetValue(configurationKey, out value);
    }

    #endregion IConfiguration

    /// <inheritdoc/>
    public bool TryGetOverride(ResourceId id, out RuleOverride? value)
    {
        value = default;
        if (_Override == null) return false;

        return _Override.TryGetValue(id.Value, out value) ||
            _Override.TryGetValue(id.Name, out value);
    }

    public ITargetBindingResult Bind(ITargetObject targetObject)
    {
        if (_TargetBinder == null) throw new InvalidOperationException($"Run '{Guid}': Target binder is not configured.");

        return _TargetBinder.Bind(targetObject);
    }

    public void Configure(OptionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // _Configuration = context.Configuration;
        // _Configuration ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        // WithFilter(context.RuleFilter);
        // WithFilter(context.ConventionFilter);
        // _BindingComparer = context.Binding.GetComparer();
        Culture = context.Output.Culture;

        // var builder = new TargetBinderBuilder(context.BindTargetName, context.BindTargetType, context.BindField, context.InputTargetType);
        // _TargetBinder = builder.Build(context.Binding);
        _Override = WithOverride(context.Override);
    }

    private static WildcardMap<RuleOverride>? WithOverride(OverrideOption option)
    {
        if (option == null || option.Level == null)
            return default;

        var overrides = option.Level
            .Where(l => l.Value != SeverityLevel.None)
            .Select(l => new KeyValuePair<string, RuleOverride>(l.Key, new RuleOverride { Level = l.Value }));

        return new WildcardMap<RuleOverride>(overrides);
    }
}
