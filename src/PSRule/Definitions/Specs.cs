// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;

namespace PSRule.Definitions;

internal static class Specs
{
    /// <summary>
    /// The API version for V1 resources.
    /// </summary>
    internal const string V1 = "github.com/microsoft/PSRule/v1";

    /// <summary>
    /// The API version for 2025-01-01 resources.
    /// </summary>
    internal const string API_2025_01_01 = "github.com/microsoft/PSRule/2025-01-01";

    // Resource names for different types of resources.
    internal const string Rule = "Rule";
    internal const string Baseline = "Baseline";
    internal const string ModuleConfig = "ModuleConfig";
    internal const string Selector = "Selector";
    internal const string SuppressionGroup = "SuppressionGroup";

    /// <summary>
    /// The built-in resource types.
    /// </summary>
    public static readonly ISpecDescriptor[] BuiltinTypes =
    [
        new SpecDescriptor<RuleV1, RuleV1Spec>(V1, Rule),
        new SpecDescriptor<Baseline, BaselineSpec>(V1, Baseline),
        new SpecDescriptor<ModuleConfigV1, ModuleConfigV1Spec>(V1, ModuleConfig),
        new SpecDescriptor<SelectorV1, SelectorV1Spec>(V1, Selector),
        new SpecDescriptor<SuppressionGroupV1, SuppressionGroupV1Spec>(V1, SuppressionGroup),

        // 2025-01-01
        new SpecDescriptor<RuleV1, RuleV1Spec>(API_2025_01_01, Rule),
        new SpecDescriptor<Baseline, BaselineSpec>(API_2025_01_01, Baseline),
        new SpecDescriptor<ModuleConfigV2, ModuleConfigV2Spec>(API_2025_01_01, ModuleConfig),
        new SpecDescriptor<SelectorV2, SelectorV2Spec>(API_2025_01_01, Selector),
        new SpecDescriptor<SuppressionGroupV2, SuppressionGroupV2Spec>(API_2025_01_01, SuppressionGroup)
    ];
}
