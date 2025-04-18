// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Runtime.Binding;

namespace PSRule.Pipeline.Runs;

#nullable enable

/// <summary>
/// An instance of a run.
/// </summary>
[DebuggerDisplay("{Id}")]
internal sealed class Run(string id, InfoString description, string correlationGuid) : IRun
{
    private ITargetBinder _TargetBinder;
    private WildcardMap<RuleOverride>? _Override;

    /// <summary>
    /// A unique identifier for the run.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// A correlation identifier for all related runs.
    /// </summary>
    public string CorrelationGuid { get; } = correlationGuid;

    /// <summary>
    /// A description of the logical run.
    /// </summary>
    public InfoString Description { get; } = description;

    /// <summary>
    /// The results from the run.
    /// </summary>
    public InvokeResult? Result { get; set; }

    /// <summary>
    /// Get an ordered culture preference list which will be tries for finding help.
    /// </summary>
    public string[]? Culture { get; private set; }

    #region IConfiguration

    /// <inheritdoc/>
    public object? GetValueOrDefault(string configurationKey, object? defaultValue = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string configurationKey, out object? value)
    {
        throw new NotImplementedException();
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

    public void Configure(OptionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // _Configuration = context.Configuration;
        // _Configuration ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        // WithFilter(context.RuleFilter);
        // WithFilter(context.ConventionFilter);
        // _BindingComparer = context.Binding.GetComparer();
        Culture = context.Output.Culture;

        var builder = new TargetBinderBuilder(context.BindTargetName, context.BindTargetType, context.BindField, context.InputTargetType);
        _TargetBinder = builder.Build(context.Binding);
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

#nullable restore
