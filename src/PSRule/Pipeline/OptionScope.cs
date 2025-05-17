// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Options;

namespace PSRule.Pipeline;

internal enum ScopeType
{
    /// <summary>
    /// Used when options are passed in by command line parameters.
    /// </summary>
    Parameter = 0,

    /// <summary>
    /// Used when a baseline is explicitly set by name.
    /// </summary>
    Explicit = 1,

    /// <summary>
    /// Used when options are set within the PSRule options from the workspace or an options object.
    /// </summary>
    Workspace = 2,

    /// <summary>
    /// 
    /// </summary>
    Baseline = 3,

    /// <summary>
    /// Used for options that are inherited from module configuration.
    /// </summary>
    Module = 4
}

[DebuggerDisplay("{Type} - {LanguageScope}")]
internal class OptionScope
{
    public readonly ScopeType Type;
    public readonly string LanguageScope;

    private OptionScope(ScopeType type, string languageScope)
    {
        Type = type;
        LanguageScope = ResourceHelper.NormalizeScope(languageScope);
    }

    public Options.BaselineOption Baseline { get; set; }

    public BindingOption Binding { get; set; }

    public ConfigurationOption Configuration { get; set; }

    public ConventionOption Convention { get; set; }

    public ExecutionOption Execution { get; set; }

    public IncludeOption Include { get; set; }

    public InputOption Input { get; set; }

    public OutputOption Output { get; set; }

    public OverrideOption Override { get; set; }

    public RepositoryOption Repository { get; set; }

    public RequiresOption Requires { get; set; }

    public RuleOption Rule { get; set; }

    public SuppressionOption Suppression { get; set; }

    public static OptionScope FromParameters(string[] ruleInclude, Hashtable ruleTag, string[] conventionInclude)
    {
        bool? includeLocal = ruleInclude == null && ruleTag == null ? null : false;

        return new OptionScope(ScopeType.Parameter, null)
        {
            Rule = new RuleOption
            {
                Include = ruleInclude,
                Tag = ruleTag,
                IncludeLocal = includeLocal,
            },
            Convention = new ConventionOption
            {
                Include = conventionInclude
            }
        };
    }

    public static OptionScope FromWorkspace(PSRuleOption option)
    {
        return new OptionScope(ScopeType.Workspace, null)
        {
            Baseline = option.Baseline,
            Binding = option.Binding,
            Configuration = option.Configuration,
            Convention = option.Convention,
            Execution = option.Execution,
            Include = option.Include,
            Input = option.Input,
            Output = option.Output,
            Override = option.Override,
            Repository = option.Repository,
            Requires = option.Requires,
            Rule = option.Rule,
            Suppression = option.Suppression
        };
    }

    public static OptionScope FromModuleConfig(string module, IModuleConfigSpec spec)
    {
        switch (spec)
        {
            case IModuleConfigV1Spec v1:
                return new OptionScope(ScopeType.Module, module)
                {
                    Binding = v1.Binding,
                    Configuration = v1.Configuration,
                    Convention = ApplyScope(module, v1.Convention),
                    Output = new OutputOption
                    {
                        Culture = v1.Output?.Culture
                    },
                    Rule = new RuleOption
                    {
                        Baseline = v1.Rule?.Baseline
                    }
                };

            case IModuleConfigV2Spec v2:
                return new OptionScope(ScopeType.Module, module)
                {
                    Binding = v2.Binding,
                    //
                    Configuration = v2.Configuration,
                    Convention = ApplyScope(module, v2.Convention),
                    Output = new OutputOption
                    {
                        Culture = v2.Output?.Culture
                    },
                    Rule = new RuleOption
                    {
                        Baseline = v2.Rule?.Baseline
                    }
                };
        }

        throw new InvalidOperationException();
    }

    internal static OptionScope FromBaseline(ScopeType type, string baselineId, string module, BaselineSpec spec, bool obsolete)
    {
        return new OptionScope(type, module)
        {
            Configuration = spec.Configuration,
            Rule = new RuleOption
            {
                Include = spec.Rule?.Include,
                Exclude = spec.Rule?.Exclude,
                Tag = spec.Rule?.Tag,
                Labels = spec.Rule?.Labels,
                IncludeLocal = type == ScopeType.Explicit ? false : null
            },
            Override = spec.Override
        };
    }

    private static string[]? GetConventions(string scope, string[] include)
    {
        if (include == null || include.Length == 0)
            return null;

        for (var i = 0; i < include.Length; i++)
            include[i] = ResourceHelper.GetIdString(scope, include[i]);

        return include;
    }

    private static ConventionOption ApplyScope(string scope, ConventionOption option)
    {
        if (option == null || option.Include == null || option.Include.Length == 0)
            return option;

        option.Include = GetConventions(scope, option.Include);
        return option;
    }
}
