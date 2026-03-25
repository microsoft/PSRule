// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Host;
using PSRule.Options;
using PSRule.Rules;
using PSRule.Runtime;
using PSRule.Runtime.Binding;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A builder to create a <see cref="RunCollection"/>.
/// </summary>
internal sealed class RunCollectionBuilder(IResourceCache resourceCache, ILogger? logger, PSRuleOption? option, ILanguageScopeSet languageScopeSet, string instance)
{
    private const char SLASH = '/';
    private const char SPACE = ' ';
    private const char DOT = '.';

    private readonly IResourceCache _ResourceCache = resourceCache ?? throw new ArgumentNullException(nameof(resourceCache));
    private readonly ILogger _Logger = logger ?? new NullLogger();
    private readonly string _Category = NormalizeCategory(option?.Run?.Category);
    private readonly string _Description = option?.Run?.Description ?? RunOption.Default.Description!;
    private readonly string _Instance = instance ?? throw new ArgumentNullException(nameof(instance));

    /// <summary>
    /// A correlation identifier for all related runs.
    /// </summary>
    private readonly string _CorrelationGuid = Guid.NewGuid().ToString();

    private readonly RunCollection _Runs = [];

    /// <summary>
    /// Build a <see cref="RunCollection"/>.
    /// </summary>
    public RunCollection Build()
    {
        _Logger.LogDebug(EventId.None, "Built {0} runs with a total of {1} rules.", _Runs.Count, _Runs.RuleCount);

        return _Runs;
    }

    public RunCollectionBuilder WithBaselinesOrDefault(LegacyRunspaceContext context)
    {
        if (context.Pipeline.Baselines != null && context.Pipeline.Baselines.Length > 0)
        {
            foreach (var baseline in context.Pipeline.Baselines)
            {
                WithBaselines([baseline.Name]);
            }
        }
        else
        {
            return WithDefaultRun(context);
        }
        return this;
    }

    internal RunCollectionBuilder WithDefaultRun(LegacyRunspaceContext context)
    {
        _Logger.LogDebug(EventId.None, "Building with default run.");

        WithImplicitBaselines();

        var id = NormalizeId(_Category, string.Empty, _Instance);

        _Logger.LogDebug(EventId.None, "Building default run as {0}.", id);

        // Get baseline configuration.
        var configuration = BuildRunConfiguration(id, null, null, option);

        var ignoredScopes = _Runs.Select(r => r.Scope).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        var filter = context.Match;

        if (_Runs.Count > 0)
        {
            filter = resource => context.Match(resource) && resource.Id.Scope != null && !ignoredScopes.Contains(resource.Id.Scope);
        }

        // Rules might be added, don't add rules for any implicit baseline scopes.
        var graph = HostHelper.GetRuleBlockGraphV2(context, filter, _ResourceCache);

        var result = new Run(
            logger: _Logger,
            scope: null,
            id: id,
            description: new InfoString(_Description),
            correlationGuid: _CorrelationGuid,
            graph: new RuleGraph(graph),
            configuration: configuration,
            targetBinder: BuildTargetBinder(id, null, option)
        );

        _Logger.LogDebug(EventId.None, "Adding run '{0}' for default run with {1} rules.", result.Guid, graph.Count);

        _Runs.Add(result);

        return this;
    }

    private RunCollectionBuilder WithImplicitBaselines()
    {
        var configs = _ResourceCache.GetType<IModuleConfig>();

        foreach (var config in configs)
        {
            if (config == null || config.Spec == null)
                continue;

            switch (config.Spec)
            {
                case IModuleConfigV2Spec v2 when v2?.Rule?.Baseline != null && _ResourceCache.TryGet<Baseline>(GetScopedBaselineId(config.Name, v2.Rule.Baseline.Value).Value, out var baseline) && baseline != null:
                    WithBaseline(baseline, config.Name);
                    break;

                case IModuleConfigV1Spec v1 when v1?.Rule?.Baseline != null && _ResourceCache.TryGet<Baseline>(GetScopedBaselineId(config.Name, v1.Rule.Baseline.Value).Value, out var baseline) && baseline != null:
                    WithBaseline(baseline, config.Name);
                    break;
            }
        }

        return this;
    }

    private static ResourceId GetScopedBaselineId(string defaultScope, ResourceIdReference reference)
    {
        return reference.AsResourceId(ResourceIdKind.Unknown, defaultScope);
    }

    /// <summary>
    /// Add explicit baselines to the run collection.
    /// </summary>
    /// <param name="names">The names of the specified baselines.</param>
    internal RunCollectionBuilder WithBaselines(string[] names)
    {
        var set = names.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var baseline in _ResourceCache.OfType<Baseline>().Where(b => set.Contains(b.BaselineId)))
        {
            WithBaseline(baseline, null);
        }

        return this;
    }

    /// <summary>
    /// Include a baseline in the run collection.
    /// </summary>
    /// <param name="baseline">The baseline.</param>
    /// <param name="implicitScope">When set for implicit baselines, if the include rule options are not set, then default to scope\*.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If a null baseline is provided.</exception>
    private RunCollectionBuilder WithBaseline(Baseline baseline, string? implicitScope)
    {
        if (baseline == null) throw new ArgumentNullException(nameof(baseline));

        var id = NormalizeId(_Category, baseline.Name, _Instance);

        _Logger.LogDebug(EventId.None, "Building run for baseline {0} as {1}.", baseline.Id, id);

        if (baseline.Obsolete)
        {
            _Logger.WarnResourceObsolete(ResourceKind.Baseline, baseline.Id.ToString());
        }

        // Get module configuration.
        var parentConfig = _ResourceCache.OfType<IScopeConfig>().FirstOrDefault(c => StringComparer.OrdinalIgnoreCase.Equals(c.Id.Name, baseline.Id.Scope));

        if (parentConfig == null)
        {
            _Logger.LogDebug(EventId.None, "No module configuration found for baseline '{0}'.", baseline.Id);
        }

        var ruleOption = RuleOption.Combine(option?.Rule, baseline.Spec.Rule);

        // TODO: We should provide a more flexible delegate to configure this.
        if (ruleOption.Include == null && implicitScope != null)
        {
            _Logger.LogDebug(EventId.None, "Using implicit scope '{0}' for baseline '{1}'.", implicitScope, baseline.Id);
        }

        var filter = new RuleFilter(ruleOption.Include, ruleOption.Tag, ruleOption.Exclude, ruleOption.IncludeLocal, ruleOption.Labels, implicitScope);

        var context = new RunCollectionBuilderContext(_Logger, option, languageScopeSet, filter);

        // Get baseline configuration.
        var configuration = BuildRunConfiguration(id, baseline?.Spec?.Configuration, parentConfig, option);

        _Logger.LogDebug(EventId.None, "Built configuration '{0}' for baseline '{1}' with {2} items.", configuration.Guid, baseline.Id, configuration.Configuration.Count);

        var graph = GetRuleGraph(context, _ResourceCache);

        _Logger.LogDebug(EventId.None, "Added {0} rules for baseline '{1}' to '{2}'.", graph.Count, baseline.Id, id);

        var result = new Run(
            logger: _Logger,
            scope: baseline.Id.Scope,
            id: id,
            description: baseline?.Info?.Description != null && baseline.Info.Description.HasValue ? baseline.Info.Description : new InfoString(_Description),
            correlationGuid: _CorrelationGuid,
            graph: new RuleGraph(graph),
            configuration: configuration,
            targetBinder: BuildTargetBinder(id, parentConfig, option)
        );

        _Logger.LogDebug(EventId.None, "Adding run '{0}' for baseline '{1}'.", result.Guid, baseline.Id);

        _Runs.Add(result);

        return this;
    }

    private RunConfiguration BuildRunConfiguration(string id, ConfigurationOption? baselineConfig, IScopeConfig? scopeConfig, PSRuleOption? options)
    {
        var configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Add items based on last one wins.
        if (scopeConfig != null && scopeConfig.Configuration != null)
        {
            foreach (var kv in scopeConfig.Configuration)
            {
                _Logger.LogDebug(EventId.None, "Adding module configuration key '{0}' to '{1}'.", kv.Key, id);
                configuration[kv.Key] = kv.Value;
            }
        }

        if (baselineConfig != null)
        {
            foreach (var kv in baselineConfig)
            {
                _Logger.LogDebug(EventId.None, "Adding baseline configuration key '{0}' to '{1}'.", kv.Key, id);
                configuration[kv.Key] = kv.Value;
            }
        }

        if (options != null && options.Configuration != null)
        {
            foreach (var kv in options.Configuration)
            {
                _Logger.LogDebug(EventId.None, "Adding local configuration key '{0}' to '{1}'.", kv.Key, id);
                configuration[kv.Key] = kv.Value;
            }
        }

        return new RunConfiguration(
            configuration
        );
    }

    private static ITargetBinder BuildTargetBinder(string id, IScopeConfig? scopeConfig, PSRuleOption? options)
    {
        var binding = BindingOption.Combine(scopeConfig?.Binding, options?.Binding);
        var inputTargetType = options?.Input?.TargetType;

        var builder = new TargetBinderBuilder(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, inputTargetType);
        return builder.Build(binding);
    }

    private static DependencyGraph<IRuleBlock> GetRuleGraph(IRunBuilderContext context, IResourceCache resourceCache)
    {
        var rules = resourceCache.OfType<IRuleV1>();
        var blocks = rules.ToRuleDependencyTargetCollectionV2(context, skipDuplicateName: false);

        var builder = new DependencyGraphBuilder<IRuleBlock>(context, includeDependencies: true, includeDisabled: false);
        builder.Include(blocks, filter: context.Match);
        return builder.Build();
    }

    /// <summary>
    /// Trim out any leading or trailing whitespace, slashes, or dots.
    /// </summary>
    private static string NormalizeCategory(string? category)
    {
        var result = category?.TrimStart(SPACE, SLASH)?.TrimEnd(SPACE, SLASH, DOT);
        return string.IsNullOrWhiteSpace(result) ? RunOption.Default.Category! : result!;
    }

    /// <summary>
    /// Normalize the run identifier to remove segments that are not required.
    /// For example: <c>NormalizeId("Category", "Name", "Instance") => "Category/Name/Instance"</c>
    /// </summary>
    /// <param name="category">The category of the run.</param>
    /// <param name="name">An optional name of the run. The name is ignored if it is empty, whitespace, or <c>.</c>.</param>
    /// <param name="instance">The instance of the run.</param>
    /// <returns>A formatted string with each segment separated by a <c>/</c>.</returns>
    private static string NormalizeId(string category, string name, string instance)
    {
        return name == "." || string.IsNullOrWhiteSpace(name)
            ? string.Concat(category, SLASH, instance)
            : string.Concat(category, SLASH, name, SLASH, instance);
    }
}
